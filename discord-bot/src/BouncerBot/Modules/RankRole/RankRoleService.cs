using BouncerBot;
using Humanizer;
using Microsoft.Extensions.Logging;
using NetCord;

namespace BouncerBot.Modules.RankRole;

public interface IRankRoleService
{
    /// <summary>
    /// Ensures that only the highest rank role is kept for the user.
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task CleanupRankRolesAsync(GuildUser guildUser);
}

internal class RankRoleService(
    ILogger<RankRoleService> logger,
    IDiscordGatewayClient gatewayClient,
    IDiscordRestClient restClient
    ) : IRankRoleService
{
    /// <inheritdoc />
    public async Task CleanupRankRolesAsync(GuildUser guildUser)
    {
        var guild = gatewayClient.Cache.Guilds[guildUser.GuildId];
        if (guild is null)
            return;

        // Get all rank roles that exist in the guild.
        // Rank name must match humanized enum name

        var rankEnumNames = Enum.GetValues<Rank>().Select(rank => new {
            Name = Enum.GetName(rank)!,
            FormalName = rank.Humanize()
        });

        var rankRoles = guild.Roles
            .Where(role =>
                rankEnumNames.Any(rn =>
                    rn.FormalName.Equals(role.Value.Name, StringComparison.OrdinalIgnoreCase) ||
                    rn.Name.Equals(role.Value.Name, StringComparison.OrdinalIgnoreCase)
            ))
            .ToDictionary(role => role.Key, role => role.Value);

        if (rankRoles.Count == 0)
            return;

        // Get rank roles that the user currently has
        var userRankRoles = rankRoles
            .Where(role => guildUser.RoleIds.Contains(role.Key))
            .ToList();

        if (userRankRoles.Count <= 1)
            return; // User has 0 or 1 rank role, no cleanup needed

        logger.LogInformation("Detected multiple rank roles for user {UserId} in guild {GuildId}: {RoleCount} roles. Cleaning up.",
            guildUser.Id, guildUser.GuildId, userRankRoles.Count);

        var rolesToRemove = userRankRoles
            .OrderByDescending(r => r.Value.Position)
            .Select(r => r.Key)
            .Skip(1);

        foreach (var roleId in rolesToRemove)
        {
            await restClient.RemoveGuildUserRoleAsync(guildUser.GuildId, guildUser.Id, roleId);
        }
    }
}
