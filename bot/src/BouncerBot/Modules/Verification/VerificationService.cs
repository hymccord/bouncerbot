using BouncerBot;
using BouncerBot.Db;
using BouncerBot.Db.Models;
using BouncerBot.Modules.Bounce;
using BouncerBot.Modules.Privacy.Modules;
using BouncerBot.Rest;
using BouncerBot.Rest.Models;
using BouncerBot.Services;
using Humanizer;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BouncerBot.Modules.Verification;

public interface IVerificationService
{
    Task<VerificationAddResult> AddVerifiedUserAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<CanUserVerifyResult> CanUserVerifyAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<bool> IsDiscordUserVerifiedAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<bool> HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync(uint mousehuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<bool> HasDiscordUserVerifiedBeforeAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<VerificationRemoveResult> RemoveVerifiedUser(ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<VerificationRemoveResult> RemoveVerificationHistoryAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task SetVerificationMessageAsync(SetVerificationMessageParameters parameters);
}

public class VerificationService(
    ILogger<VerificationService> logger,
    BouncerBotDbContext dbContext,
    ICommandMentionService commandMentionService,
    IDiscordRestClient restClient,
    IGuildLoggingService guildLoggingService,
    IMouseHuntRestClient mouseHuntRestClient,
    IBounceService bounceService) : IVerificationService
{
    private Title[]? _cachedTitles;

    public async Task<VerificationAddResult> AddVerifiedUserAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId, cancellationToken: cancellationToken);
        if (existingUser is null)
        {
            await dbContext.VerifiedUsers.AddAsync(new VerifiedUser
            {
                MouseHuntId = mouseHuntId,
                GuildId = guildId,
                DiscordId = discordId
            }, cancellationToken);

            // Add to verification history
            var mhIdHash = VerificationHistory.HashValue(mouseHuntId);
            var discordIdHash = VerificationHistory.HashValue(discordId);
            var existingHistory = await dbContext.VerificationHistory
                .FirstOrDefaultAsync(vh => vh.GuildId == guildId && vh.DiscordIdHash == discordIdHash, cancellationToken);

            if (existingHistory is null)
            {
                await dbContext.VerificationHistory.AddAsync(new VerificationHistory
                {
                    GuildId = guildId,
                    DiscordIdHash = discordIdHash,
                    MouseHuntIdHash = mhIdHash,
                }, cancellationToken);
            }
            else
            {
                // Sanity check hash matches
                if (existingHistory.MouseHuntIdHash != mhIdHash)
                {
                    logger.LogWarning("User {UserId} in guild {GuildId} has a different MouseHunt ID hash ({OldHash} vs {NewHash})",
                        discordId, guildId, existingHistory.MouseHuntIdHash, mhIdHash);

                    throw new InvalidOperationException("MouseHunt ID for user doesn't match historical value.");
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            logger.LogDebug("User {UserId} is already verified in guild {GuildId}", discordId, guildId);
        }

        return new VerificationAddResult
        {
            WasAdded = existingUser is null,
            MouseHuntId = mouseHuntId
        };
    }

    public async Task<bool> IsDiscordUserVerifiedAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
        => await dbContext.VerifiedUsers
            .AnyAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId, cancellationToken: cancellationToken);

    public async Task<CanUserVerifyResult> CanUserVerifyAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        // Check verification restrictions in priority order

        // Check if the MouseHunt ID is banned
        var banCheck = await CheckHunterIdBanned(mouseHuntId, guildId, cancellationToken);
        if (!banCheck.CanVerify)
        {
            _ = guildLoggingService.LogErrorAsync(guildId, "Verification Blocked", $"""
                :warning: <@{discordId}> attempted to verify with a banned MouseHunt ID ([{mouseHuntId}](<https://p.mshnt.ca/{mouseHuntId}>)). :warning:
                """, default);
            return banCheck;
        }

        var existingUserCheck = await CheckHunterInUseAsync(mouseHuntId, guildId, discordId, cancellationToken);
        if (!existingUserCheck.CanVerify)
        {
            _ = guildLoggingService.LogWarningAsync(guildId, "Verification Blocked", $"""
                <@{discordId}> attempted to verify with a MouseHunt ID ([{mouseHuntId}](<https://p.mshnt.ca/{mouseHuntId}>)) that is already in use.
                """, default);
            return existingUserCheck;
        }

        var historyCheck = await CheckUserVerificationHistoryAsync(mouseHuntId, guildId, discordId, cancellationToken);
        if (!historyCheck.CanVerify)
        {
            _ = guildLoggingService.LogWarningAsync(guildId, "Verification Blocked", $"""
                <@{discordId}> attempted to verify with a different MouseHunt ID ([{mouseHuntId}](<https://p.mshnt.ca/{mouseHuntId}>)).
                """, default);
            return historyCheck;
        }

        historyCheck = await CheckMouseHuntIdVerificationHistoryAsync(mouseHuntId, guildId, discordId, cancellationToken);
        if (!historyCheck.CanVerify)
        {
            _ = guildLoggingService.LogWarningAsync(guildId, "Verification Blocked", $"""
                <@{discordId}> attempted to verify with a MouseHunt ID ([{mouseHuntId}](<https://p.mshnt.ca/{mouseHuntId}>)) belonging to a different Discord user.
                """, default);
            return historyCheck;
        }

        var rankCheck = await CheckUserRankRequirementAsync(mouseHuntId, guildId, cancellationToken);
        if (!rankCheck.CanVerify)
        {
            _ = guildLoggingService.LogWarningAsync(guildId, "Verification Blocked", $"""
                <@{discordId}> attempted to verify with a MouseHunt ID that does not meet the rank requirement: [{mouseHuntId}](<https://p.mshnt.ca/{mouseHuntId}>).
                """, default);
            return rankCheck;
        }

        return new CanUserVerifyResult
        {
            CanVerify = true,
            Message = "",
        };
    }

    private async Task<CanUserVerifyResult> CheckHunterIdBanned(uint mouseHuntId, ulong guildId, CancellationToken cancellationToken)
    {
        var bannedHunter = await bounceService.GetBannedHunterAsync(mouseHuntId, guildId);
        if (bannedHunter != null)
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = """
                Sorry, an internal error occurred. Try again later.

                Please contact the moderators with error code: `VF-2156-B3`.
                """
            };
        }

        return new CanUserVerifyResult { CanVerify = true, Message = "" };
    }

    private async Task<CanUserVerifyResult> CheckHunterInUseAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.GuildId == guildId && vu.MouseHuntId == mouseHuntId && vu.DiscordId != discordId, cancellationToken);

        if (existingUser is not null)
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = """
                Sorry, an internal error occured. Try again later.

                Please contact the moderators with error code: `VF-8429-A7`.
                """
            };
        }

        return new CanUserVerifyResult { CanVerify = true, Message = "" };
    }

    private async Task<CanUserVerifyResult> CheckUserVerificationHistoryAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken)
    {
        var discordIdHash = VerificationHistory.HashValue(discordId);
        var previousVerification = await dbContext.VerificationHistory
            .FirstOrDefaultAsync(vh => vh.GuildId == guildId && vh.DiscordIdHash == discordIdHash, cancellationToken);

        if (previousVerification is not null && !previousVerification.VerifyMouseHuntId(mouseHuntId))
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = $"""
                You previously verified a different MouseHunt ID in this server.

                Please contact the moderators if you think this is an error or to explain why a change is required.

                -# To learn how I know this, use the {commandMentionService.GetCommandMention(PrivacyModuleMetadata.PrivacyCommand.Name)} command.
                """
            };
        }

        return new CanUserVerifyResult { CanVerify = true, Message = "" };
    }

    private async Task<CanUserVerifyResult> CheckMouseHuntIdVerificationHistoryAsync(uint mouseHuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken)
    {
        var mhIdHash = VerificationHistory.HashValue(mouseHuntId);
        var previousVerification = await dbContext.VerificationHistory
            .FirstOrDefaultAsync(vh => vh.GuildId == guildId && vh.MouseHuntIdHash == mhIdHash, cancellationToken);
        if (previousVerification is not null && !previousVerification.VerifyDiscordId(discordId))
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = $"""
                Sorry, an internal error occurred. Try again later.

                Please contact the moderators with error code: `VF-9518-C3`.
                """
            };
        }
        return new CanUserVerifyResult { CanVerify = true, Message = "" };
    }

    private async Task<CanUserVerifyResult> CheckUserRankRequirementAsync(uint mouseHuntId, ulong guildId, CancellationToken cancellationToken)
    {
        var minRank = await GetMinimumRankForGuildAsync(guildId, cancellationToken);
        var userRank = await GetUserRankAsync(mouseHuntId, cancellationToken);

        if (userRank < minRank)
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = $"You're a little too new around here! Rank up to **{minRank.Humanize()}** and I'll reconsider."
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

    public async Task<bool> HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync(uint mousehuntId, ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var discordIdHash = VerificationHistory.HashValue(discordId);
        var existingHistory = await dbContext.VerificationHistory
            .FirstOrDefaultAsync(vh => vh.GuildId == guildId && vh.DiscordIdHash == discordIdHash, cancellationToken);

        return existingHistory?.VerifyMouseHuntId(mousehuntId) ?? false;
    }

    public async Task<bool> HasDiscordUserVerifiedBeforeAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var discordIdHash = VerificationHistory.HashValue(discordId);
        return await dbContext.VerificationHistory
            .AnyAsync(vh => vh.GuildId == guildId && vh.DiscordIdHash == discordIdHash, cancellationToken);
    }

    public async Task<VerificationRemoveResult> RemoveVerifiedUser(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var existingUser = await dbContext.VerifiedUsers
            .Include(vu => vu.VerifyMessage)
            .FirstOrDefaultAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId, cancellationToken: cancellationToken);
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
                catch (NetCord.Rest.RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogInformation("Verification message {MessageId} in channel {ChannelId} for user {UserId} in guild {GuildId} was already deleted",
                        verifyMessage.MessageId,
                        verifyMessage.ChannelId,
                        discordId,
                        guildId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to delete verification message {MessageId} in channel {ChannelId} for user {UserId} in guild {GuildId}",
                        verifyMessage.MessageId,
                        verifyMessage.ChannelId,
                        discordId,
                        guildId);
                }
                dbContext.VerifyMessages.Remove(existingUser.VerifyMessage);
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new VerificationRemoveResult
        {
            WasRemoved = existingUser is not null,
        };
    }

    public async Task<VerificationRemoveResult> RemoveVerificationHistoryAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var discordIdHash = VerificationHistory.HashValue(discordId);
        var existingHistory = await dbContext.VerificationHistory
            .FirstOrDefaultAsync(vh => vh.GuildId == guildId && vh.DiscordIdHash == discordIdHash, cancellationToken);
        if (existingHistory is not null)
        {
            dbContext.VerificationHistory.Remove(existingHistory);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new VerificationRemoveResult
        {
            WasRemoved = existingHistory is not null
        };
    }

    public async Task SetVerificationMessageAsync(SetVerificationMessageParameters parameters)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == parameters.DiscordUserId && vu.GuildId == parameters.GuildId);

        if (existingUser is null)
        {
            logger.LogWarning("Attempted to set verification message {MessageId} in channel {ChannelId} for user {UserId} in guild {GuildId}, but they are not verified.",
                parameters.MessageId,
                parameters.ChannelId,
                parameters.DiscordUserId,
                parameters.GuildId);
            return;
        }

        existingUser.VerifyMessage = new VerifyMessage
        {
            ChannelId = parameters.ChannelId,
            MessageId = parameters.MessageId
        };

        await dbContext.SaveChangesAsync();
    }
}

public readonly record struct SetVerificationMessageParameters(
    ulong GuildId,
    ulong DiscordUserId,
    ulong ChannelId,
    ulong MessageId
);

public readonly record struct CanUserVerifyResult(bool CanVerify, string Message);

public readonly record struct VerificationAddResult(bool WasAdded, uint MouseHuntId);

public readonly record struct VerificationRemoveResult(bool WasRemoved);
