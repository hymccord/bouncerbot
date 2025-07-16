using BouncerBot.Db;

using NetCord.Gateway;

namespace BouncerBot.Modules.Achieve;

public class AchievementRoleService(GatewayClient gatewayClient, BouncerBotDbContext dbContext)
{
    public async Task AddRoleAsync(ulong userId, ulong guildId, AchievementRole role, CancellationToken cancellationToken = default)
    {
        var roleId = await GetRoleIdAsync(guildId, role)
            ?? throw new InvalidOperationException($"The role for this achievement has not been configured yet. An admin needs to use `/config role`.");
        var guild = gatewayClient.Cache.Guilds[guildId]
            ?? throw new InvalidOperationException($"I was unable to find the server in my cache.");
        var user = guild.Users[userId]
            ?? throw new InvalidOperationException($"I was unable to find the user in my cache.");

        if (!user.RoleIds.Contains(roleId))
        {
            await user.AddRoleAsync(roleId, cancellationToken: cancellationToken);
        }
    }

    public async Task RemoveRoleAsync(ulong userId, ulong guildId, AchievementRole role, CancellationToken cancellationToken = default)
    {
        var roleId = await GetRoleIdAsync(guildId, role)
            ?? throw new InvalidOperationException($"The role for this achievement has not been configured yet. An admin needs to use `/config role`.");
        var guild = gatewayClient.Cache.Guilds[guildId]
            ?? throw new InvalidOperationException($"I was unable to find the server in my cache.");
        var user = guild.Users[userId]
            ?? throw new InvalidOperationException($"I was unable to find the user in my cache.");

        if (user.RoleIds.Contains(roleId))
        {
            await user.RemoveRoleAsync(roleId, cancellationToken: cancellationToken);
        }
    }

    public async Task<ulong?> GetRoleIdAsync(ulong guildId, AchievementRole role)
    {
        return await dbContext.RoleSettings.FindAsync(guildId) switch
        {
            { StarId: var id } when role == AchievementRole.Star => id,
            { CrownId: var id } when role == AchievementRole.Crown => id,
            { CheckmarkId: var id } when role == AchievementRole.Checkmark => id,
            { EggMasterId: var id } when role == AchievementRole.EggMaster => id,
            { ArcaneMasterId: var id } when role == AchievementRole.ArcaneMaster => id,
            { DraconicMasterId: var id } when role == AchievementRole.DraconicMaster => id,
            { ForgottenMasterId: var id } when role == AchievementRole.ForgottenMaster => id,
            { HydroMasterId: var id } when role == AchievementRole.HydroMaster => id,
            { LawMasterId: var id } when role == AchievementRole.LawMaster => id,
            { PhysicalMasterId: var id } when role == AchievementRole.PhysicalMaster => id,
            { RiftMasterId: var id } when role == AchievementRole.RiftMaster => id,
            { ShadowMasterId: var id } when role == AchievementRole.ShadowMaster => id,
            { TacticalMasterId: var id } when role == AchievementRole.TacticalMaster => id,
            { MultiMasterId: var id } when role == AchievementRole.MultiMaster => id,
            _ => null
        };
    }

    public async Task ResetRoleAndAddAchieverAsync(ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        var roleId = await GetRoleIdAsync(guildId, achievement)
            ?? throw new InvalidOperationException("The role for the achievement has not been configured.");
        var achieverRoleId = await dbContext.RoleSettings.FindAsync(guildId) switch
        {
            { AchieverId: var id } => id,
            _ => null
        } ?? throw new InvalidOperationException($"The achiever role has not been configured yet. An admin needs to use `/config role`.");
        var guild = gatewayClient.Cache.Guilds[guildId]
            ?? throw new InvalidOperationException($"I was unable to find the server in my cache.");
        var user = guild.Users[userId]
            ?? throw new InvalidOperationException($"I was unable to find the user in my cache.");

        if (user.RoleIds.Contains(roleId))
        {
            await user.RemoveRoleAsync(roleId, cancellationToken: cancellationToken);
        }

        if (!user.RoleIds.Contains(achieverRoleId))
        {
            await user.AddRoleAsync(achieverRoleId, cancellationToken: cancellationToken);
        }
    }
}
