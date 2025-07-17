using BouncerBot.Db;
using BouncerBot.Rest;
using BouncerBot.Rest.Models;

using Humanizer;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NetCord.Rest;

namespace BouncerBot.Modules.Verify;
public class VerificationService(ILogger<VerificationService> logger, BouncerBotDbContext dbContext, RestClient restClient, MouseHuntRestClient mouseHuntRestClient)
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

            await dbContext.SaveChangesAsync();
        }
        else
        {
            logger.LogInformation("User {UserId} is already verified in guild {GuildId}", discordId, guildId);
        }

        var roleConfig = await dbContext.RoleSettings.FindAsync(guildId);
        if (roleConfig?.VerifiedId is ulong roleId)
        {
            _ = restClient.AddGuildUserRoleAsync(guildId, discordId, roleId);
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
        // Watchlist
        // todo

        // Rank
        var minRank = await dbContext.VerifySettings
            .Where(vs => vs.GuildId == guildId)
            .Select(vs => vs.MinimumRank)
            .FirstOrDefaultAsync(cancellationToken);

        _cachedTitles ??= await mouseHuntRestClient.GetTitlesAsync(cancellationToken)
            ?? throw new Exception("Failed to fetch titles from MouseHunt API");

        var userTitle = await mouseHuntRestClient.GetUserTitleAsync(mouseHuntId, cancellationToken)
            ?? throw new Exception("Failed to fetch user title from MouseHunt API");

        var userRank = _cachedTitles.Single(t => t.TitleId == userTitle.TitleId).Name;

        if (userRank < minRank)
        {
            return new CanUserVerifyResult
            {
                CanVerify = false,
                Message = $"You're a little too new around here! Progress your rank in MouseHunt and I may reconsider."
            };
        }

        return new CanUserVerifyResult
        {
            CanVerify = true,
            Message = "",
        };
    }

    public async Task<VerificationRemoveResult> RemoveVerifiedUser(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId);
        if (existingUser is not null)
        {
            dbContext.VerifiedUsers.Remove(existingUser);
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
