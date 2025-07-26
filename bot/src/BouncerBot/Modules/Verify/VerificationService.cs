using BouncerBot.Db;
using BouncerBot.Db.Models;
using BouncerBot.Rest;
using BouncerBot.Rest.Models;

using Humanizer;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NetCord.Rest;

namespace BouncerBot.Modules.Verify;
public class VerificationService(
    ILogger<VerificationService> logger,
    BouncerBotDbContext dbContext,
    RestClient restClient,
    MouseHuntRestClient mouseHuntRestClient)
{
    private Title[]? _cachedTitles;

    public async Task<VerificationAddResult> AddVerifiedUserAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId);
        if (existingUser is null)
        {
            await dbContext.VerifiedUsers.AddAsync(new Db.Models.VerifiedUser
            {
                MouseHuntId = mouseHuntId,
                GuildId = guildId,
                DiscordId = discordId
            });

            // Add to verification history
            var hash = VerificationHistory.HashMouseHuntId(mouseHuntId);
            var existingHistory = await dbContext.VerificationHistory
                .FirstOrDefaultAsync(vh => vh.GuildId == guildId && vh.DiscordId == discordId, cancellationToken);
            
            if (existingHistory is null)
            {
                await dbContext.VerificationHistory.AddAsync(new VerificationHistory
                {
                    GuildId = guildId,
                    DiscordId = discordId,
                    MouseHuntIdHash = hash,
                }, cancellationToken);
            }
            else
            {
                // Sanity check hash matches
                if (existingHistory.MouseHuntIdHash != hash)
                {
                    logger.LogWarning("User {UserId} in guild {GuildId} has a different MouseHunt ID hash ({OldHash} vs {NewHash})", 
                        discordId, guildId, existingHistory.MouseHuntIdHash, hash);

                    throw new InvalidOperationException($"User {discordId} in guild {guildId} has a different MouseHunt ID hash ({existingHistory.MouseHuntIdHash} vs {hash})");
                }
            }

            await dbContext.SaveChangesAsync();
        }
        else
        {
            logger.LogDebug("User {UserId} is already verified in guild {GuildId}", discordId, guildId);
        }

        var roleConfig = await dbContext.RoleSettings.FindAsync(guildId);
        if (roleConfig?.VerifiedId is ulong roleId)
        {
            await restClient.AddGuildUserRoleAsync(guildId, discordId, roleId, cancellationToken: cancellationToken);
        }

        return new VerificationAddResult
        {
            WasAdded = existingUser is null,
            MouseHuntId = mouseHuntId
        };
    }

    public async Task<bool> IsDiscordUserVerifiedAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
        => await dbContext.VerifiedUsers
            .AnyAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId);

    public async Task<CanUserVerifyResult> CanUserVerifyAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        // Check verification restrictions in priority order
        
        // TODO: Check if the MouseHunt ID is banned
        
        var existingUserCheck = await CheckIfMouseHuntIdInUseAsync(mouseHuntId, guildId, cancellationToken);
        if (!existingUserCheck.CanVerify)
            return existingUserCheck;

        var historyCheck = await CheckUserVerificationHistoryAsync(mouseHuntId, guildId, discordId, cancellationToken);
        if (!historyCheck.CanVerify)
            return historyCheck;

        var rankCheck = await CheckUserRankRequirementAsync(mouseHuntId, guildId, cancellationToken);
        if (!rankCheck.CanVerify)
            return rankCheck;

        return new CanUserVerifyResult
        {
            CanVerify = true,
            Message = "",
        };
    }

    private async Task<CanUserVerifyResult> CheckIfMouseHuntIdInUseAsync(uint mouseHuntId, ulong guildId, CancellationToken cancellationToken)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.MouseHuntId == mouseHuntId && vu.GuildId == guildId, cancellationToken);
        
        if (existingUser is not null)
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = """
                This MouseHunt ID is already linked to a Discord account in this server.

                If you believe this is an error, please contact the moderators immediately.
                """
            };
        }

        return new CanUserVerifyResult { CanVerify = true, Message = "" };
    }

    private async Task<CanUserVerifyResult> CheckUserVerificationHistoryAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken)
    {
        var previousVerification = await dbContext.VerificationHistory
            .FirstOrDefaultAsync(vh => vh.GuildId == guildId && vh.DiscordId == discordId, cancellationToken);

        if (previousVerification is not null && !previousVerification.VerifyMouseHuntId(mouseHuntId))
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = """
                You have previously used a different MouseHunt ID in this server.

                If you need to change your linked account, please contact the moderators.
                """
            };
        }

        return new CanUserVerifyResult { CanVerify = true, Message = "" };
    }

    private async Task<CanUserVerifyResult> CheckUserRankRequirementAsync(uint mouseHuntId, ulong guildId, CancellationToken cancellationToken)
    {
        Rank minRank = await GetMinimumRankForGuildAsync(guildId, cancellationToken);
        Rank userRank = await GetUserRankAsync(mouseHuntId, cancellationToken);

        if (userRank < minRank)
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = $"You're a little too new around here! Rank up to {minRank.Humanize()} and I'll reconsider."
            };
        }

        return new CanUserVerifyResult { CanVerify = true, Message = "" };
    }

    private async Task<Rank> GetMinimumRankForGuildAsync(ulong guildId, CancellationToken cancellationToken)
    {
        return await dbContext.VerifySettings
            .Where(vs => vs.GuildId == guildId)
            .Select(vs => vs.MinimumRank)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Rank> GetUserRankAsync(uint mouseHuntId, CancellationToken cancellationToken)
    {
        await EnsureTitlesCachedAsync(cancellationToken);

        var userTitle = await mouseHuntRestClient.GetUserTitleAsync(mouseHuntId, cancellationToken)
            ?? throw new Exception("Failed to fetch user title from MouseHunt API");

        return _cachedTitles!.Single(t => t.TitleId == userTitle.TitleId).Name;
    }

    private async Task EnsureTitlesCachedAsync(CancellationToken cancellationToken)
    {
        _cachedTitles ??= await mouseHuntRestClient.GetTitlesAsync(cancellationToken)
            ?? throw new Exception("Failed to fetch titles from MouseHunt API");
    }

    public async Task<VerificationRemoveResult> RemoveVerifiedUser(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var existingUser = await dbContext.VerifiedUsers
            .Include(vu => vu.VerifyMessage)
            .FirstOrDefaultAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId);
        if (existingUser is not null)
        {
            // Note: We don't remove from VerificationHistory - this is intentional to prevent reuse
            dbContext.VerifiedUsers.Remove(existingUser);
            if (existingUser.VerifyMessage is VerifyMessage verifyMessage)
            {
                try
                {
                    await restClient.DeleteMessageAsync(verifyMessage.ChannelId, verifyMessage.MessageId, cancellationToken: cancellationToken);
                }
                catch
                { }
                dbContext.VerifyMessages.Remove(existingUser.VerifyMessage);
            }
            await dbContext.SaveChangesAsync();
        }

        var roleConfig = await dbContext.RoleSettings.FindAsync(guildId);
        if (roleConfig?.VerifiedId is ulong roleId)
        {
            await restClient.RemoveGuildUserRoleAsync(guildId, discordId, roleId);
        }

        return new VerificationRemoveResult
        {
            WasRemoved = existingUser is not null,
            MouseHuntId = existingUser?.MouseHuntId
        };
    }

    public async Task SetVerificationMessageAsync(SetVerificationMessageParameters parameters)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == parameters.DiscordUserId && vu.GuildId == parameters.GuildId);

        if (existingUser is null)
        {
            logger.LogWarning("Attempted to set verification message for user {UserId} in guild {GuildId}, but they are not verified.", parameters.DiscordUserId, parameters.GuildId);
            return;
        }

        existingUser.VerifyMessage = new Db.Models.VerifyMessage
        {
            ChannelId = parameters.ChannelId,
            MessageId = parameters.MessageId
        };

        await dbContext.SaveChangesAsync();
    }
}

public readonly record struct SetVerificationMessageParameters
{
    public ulong GuildId { get; init; }
    public ulong DiscordUserId { get; init; }
    public ulong ChannelId { get; init; }
    public ulong MessageId { get; init; }
}

public readonly record struct CanUserVerifyResult
{
    public bool CanVerify { get; init; }
    public required string Message { get; init; }
}

public readonly record struct VerificationAddResult
{
    public bool WasAdded { get; init; }
    public uint MouseHuntId { get; init; }
}

public readonly record struct VerificationRemoveResult
{
    public bool WasRemoved { get; init; }
    public uint? MouseHuntId { get; init; }
}
