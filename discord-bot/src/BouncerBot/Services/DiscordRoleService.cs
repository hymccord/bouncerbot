using BouncerBot.Db;
using Microsoft.EntityFrameworkCore;
using NetCord;

namespace BouncerBot.Services;

public interface IRoleService
{
    Task AddRoleAsync(ulong userId, ulong guildId, Role role, CancellationToken cancellationToken = default);
    Task<int> GetRoleUserCount(ulong guildId, Role role);
    Task<int> GetRoleUserCountWithExclude(ulong guildId, Role role, Role exclude);
    Task<ulong?> GetRoleIdAsync(ulong guildId, Role role);
    Task<bool> HasRoleAsync(ulong userId, ulong guildId, Role role);
    Task RemoveRoleAsync(ulong userId, ulong guildId, Role role, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides services for managing roles within a guild.
/// </summary>
/// <remarks>This service allows adding and removing roles for users based on achievements, retrieving role IDs
/// for specific achievements, and counting users with a specific role.</remarks>
public class DiscordRoleService(
    IDiscordGatewayClient gatewayClient,
    BouncerBotDbContext dbContext) : IRoleService
{
    public async Task AddRoleAsync(ulong userId, ulong guildId, Role role, CancellationToken cancellationToken = default)
    {
        var roleId = await GetRequiredRoleIdAsync(guildId, role);
        var user = GetGuildUser(userId, guildId);

        if (!user.RoleIds.Contains(roleId))
        {
            await user.AddRoleAsync(roleId, cancellationToken: cancellationToken);
        }
    }

    public async Task<ulong?> GetRoleIdAsync(ulong guildId, Role role)
    {
        return (await dbContext.RoleSettings.FirstOrDefaultAsync(rs => rs.GuildId == guildId && rs.Role == role))?.DiscordRoleId
            ?? throw new RoleNotConfiguredException(role);
    }

    public async Task<int> GetRoleUserCount(ulong guildId, Role role)
    {
        var roleId = await GetRequiredRoleIdAsync(guildId, role);

        return gatewayClient.Cache.Guilds[guildId]?.Users.Values
            .Count(u => u.RoleIds.Contains(roleId)) ?? 0;
    }

    public async Task<int> GetRoleUserCountWithExclude(ulong guildId, Role role, Role exclude)
    {
        var roleId = await GetRequiredRoleIdAsync(guildId, role);
        var excludeRoleId = await GetRequiredRoleIdAsync(guildId, exclude);

        return gatewayClient.Cache.Guilds[guildId]?.Users.Values
            .Count(u => u.RoleIds.Contains(roleId) && !u.RoleIds.Contains(excludeRoleId)) ?? 0;
    }

    public async Task<bool> HasRoleAsync(ulong userId, ulong guildId, Role role)
    {
        var roleId = await GetRequiredRoleIdAsync(guildId, role);
        var user = GetGuildUser(userId, guildId);

        return user.RoleIds.Contains(roleId);
    }


    public async Task RemoveRoleAsync(ulong userId, ulong guildId, Role role, CancellationToken cancellationToken = default)
    {
        var roleId = await GetRequiredRoleIdAsync(guildId, role);
        var user = GetGuildUser(userId, guildId);

        if (user.RoleIds.Contains(roleId))
        {
            await user.RemoveRoleAsync(roleId, cancellationToken: cancellationToken);
        }
    }

    private GuildUser GetGuildUser(ulong userId, ulong guildId)
    {
        if (!gatewayClient.Cache.Guilds.TryGetValue(guildId, out var guild))
        {
            throw new InvalidOperationException($"I was unable to find the server in my cache.");
        }

        if (!guild.Users.TryGetValue(userId, out var user))
        {
            throw new InvalidOperationException($"I was unable to find the user in my cache.");
        }

        return user;
    }

    private async Task<ulong> GetRequiredRoleIdAsync(ulong guildId, Role role)
    {
        return await GetRoleIdAsync(guildId, role)
            ?? throw new RoleNotConfiguredException(role);
    }
}
