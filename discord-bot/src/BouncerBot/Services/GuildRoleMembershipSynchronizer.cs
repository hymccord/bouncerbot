using BouncerBot.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetCord;

namespace BouncerBot.Services;

public interface IGuildRoleMembershipSynchronizer
{
    Task ProcessCachedUsersAsync(ulong guildId, IReadOnlyDictionary<ulong, GuildUser> guildUsers, CancellationToken cancellationToken = default);
}

internal class GuildRoleMembershipSynchronizer(
    ILogger<GuildRoleMembershipSynchronizer> logger,
    IGuildUserRoleMonitorService guildUserRoleMonitorService,
    BouncerBotDbContext dbContext
) : IGuildRoleMembershipSynchronizer
{
    private record SyncContext(ulong GuildId, ulong VerifiedRoleId, HashSet<ulong> VerifiedDiscordIds);

    public async Task ProcessCachedUsersAsync(ulong guildId, IReadOnlyDictionary<ulong, GuildUser> guildUsers, CancellationToken cancellationToken = default)
    {
        var syncContext = await GetSyncContextAsync(guildId, cancellationToken);
        if (syncContext is null)
        {
            logger.LogDebug("No verified role configured for guild {GuildId}, skipping sync", guildId);
            return;
        }

        // Process both directions concurrently
        var removeInvalidRoles = RemoveInvalidVerifiedRolesAsync(syncContext, guildUsers, cancellationToken);
        var removeUnverifiedUsers = RemoveUnverifiedUsersFromDbAsync(syncContext, guildUsers, cancellationToken);

        await Task.WhenAll(removeInvalidRoles, removeUnverifiedUsers);
    }

    private async Task<SyncContext?> GetSyncContextAsync(ulong guildId, CancellationToken cancellationToken)
    {
        var verifiedRoleId = await dbContext.RoleSettings
            .Where(rs => rs.GuildId == guildId && rs.Role == Role.Verified)
            .Select(rs => rs.DiscordRoleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (verifiedRoleId == 0)
        {
            return null;
        }

        var verifiedDiscordIds = await dbContext.VerifiedUsers
            .Where(v => v.GuildId == guildId)
            .Select(v => v.DiscordId)
            .ToHashSetAsync(cancellationToken);

        return new SyncContext(guildId, verifiedRoleId, verifiedDiscordIds);
    }

    private async Task RemoveInvalidVerifiedRolesAsync(
        SyncContext context, 
        IReadOnlyDictionary<ulong, GuildUser> guildUsers, 
        CancellationToken cancellationToken)
    {
        // Users who have the verified role but are not in the database
        var invalidUsers = guildUsers.Values
            .Where(u => u.RoleIds.Contains(context.VerifiedRoleId) && 
                       !context.VerifiedDiscordIds.Contains(u.Id))
            .ToList();

        if (invalidUsers.Count == 0)
        {
            return;
        }

        logger.LogInformation(
            "Found {Count} users in guild {GuildId} with verified role but not in database. Removing roles.",
            invalidUsers.Count, context.GuildId);

        await ProcessUsersInBatchesAsync(
            invalidUsers,
            user => guildUserRoleMonitorService.HandleRolesAddedAsync(user, [context.VerifiedRoleId]),
            "remove invalid verified roles",
            cancellationToken);
    }

    private async Task RemoveUnverifiedUsersFromDbAsync(
        SyncContext context, 
        IReadOnlyDictionary<ulong, GuildUser> guildUsers, 
        CancellationToken cancellationToken)
    {
        // Users who are in database but don't have the verified role in Discord
        var usersWithoutRole = context.VerifiedDiscordIds
            .Where(discordId => guildUsers.TryGetValue(discordId, out var user) && 
                               !user.RoleIds.Contains(context.VerifiedRoleId))
            .Select(discordId => guildUsers[discordId])
            .ToList();

        if (usersWithoutRole.Count == 0)
        {
            return;
        }

        logger.LogInformation(
            "Found {Count} users in guild {GuildId} verified in database but missing Discord role. Removing from database.",
            usersWithoutRole.Count, context.GuildId);

        await ProcessUsersInBatchesAsync(
            usersWithoutRole,
            user => guildUserRoleMonitorService.HandleRolesRemovedAsync(user, [context.VerifiedRoleId]),
            "remove users from database",
            cancellationToken);
    }

    private async Task ProcessUsersInBatchesAsync(
        IEnumerable<GuildUser> users,
        Func<GuildUser, Task> processor,
        string operation,
        CancellationToken cancellationToken,
        int batchSize = 10)
    {
        var userList = users.ToList();
        var batches = userList.Chunk(batchSize);

        foreach (var batch in batches)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Cancellation requested, stopping {Operation}", operation);
                break;
            }

            var tasks = batch.Select(async user =>
            {
                try
                {
                    await processor(user);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, 
                        "Failed to {Operation} for user {UserId} in guild {GuildId}", 
                        operation, user.Id, user.GuildId);
                }
            });

            await Task.WhenAll(tasks);

            // Small delay between batches to avoid rate limiting
            if (batchSize > 5)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
