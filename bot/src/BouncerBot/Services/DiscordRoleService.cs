using BouncerBot.Db;

using NetCord;

namespace BouncerBot.Services;

public interface IRoleService
{
    Task AddRoleAsync(ulong userId, ulong guildId, Role role, CancellationToken cancellationToken = default);
    Task<int> GetRoleUserCount(ulong guildId, Role role);
    Task<ulong?> GetRoleIdAsync(ulong guildId, Role role);
    Task<bool> HasRoleAsync(ulong userId, ulong guildId, Role Role);
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
        return await dbContext.RoleSettings.FindAsync(guildId) switch
        {
            { VerifiedId: var id } when role == Role.Verified => id,
            { StarId: var id } when role == Role.Star => id,
            { CrownId: var id } when role == Role.Crown => id,
            { CheckmarkId: var id } when role == Role.Checkmark => id,
            { EggMasterId: var id } when role == Role.EggMaster => id,
            { AchieverId: var id } when role == Role.Achiever => id,
            { ArcaneMasterId: var id } when role == Role.ArcaneMaster => id,
            { DraconicMasterId: var id } when role == Role.DraconicMaster => id,
            { ForgottenMasterId: var id } when role == Role.ForgottenMaster => id,
            { HydroMasterId: var id } when role == Role.HydroMaster => id,
            { LawMasterId: var id } when role == Role.LawMaster => id,
            { PhysicalMasterId: var id } when role == Role.PhysicalMaster => id,
            { RiftMasterId: var id } when role == Role.RiftMaster => id,
            { ShadowMasterId: var id } when role == Role.ShadowMaster => id,
            { TacticalMasterId: var id } when role == Role.TacticalMaster => id,
            { MultiMasterId: var id } when role == Role.MultiMaster => id,
            _ => throw new InvalidOperationException("Unable to find role. Please check the bot's server config."),
        } ?? throw new RoleNotConfiguredException(role);

    }

    public async Task<int> GetRoleUserCount(ulong guildId, Role role)
    {
        var roleId = await GetRequiredRoleIdAsync(guildId, role);

        return gatewayClient.Cache.Guilds[guildId]?.Users.Values
            .Count(u => u.RoleIds.Contains(roleId)) ?? 0;
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
        var guild = gatewayClient.Cache.Guilds[guildId]
            ?? throw new InvalidOperationException($"I was unable to find the server in my cache.");

        var user = guild.Users[userId]
            ?? throw new InvalidOperationException($"I was unable to find the user in my cache.");

        return user;
    }

    private async Task<ulong> GetRequiredRoleIdAsync(ulong guildId, Role role)
    {
        return await GetRoleIdAsync(guildId, role)
            ?? throw new RoleNotConfiguredException(role);
    }
}
