using NetCord;
using NetCord.Gateway;

namespace BouncerBot.Modules.Achieve;

public interface IAchievementRoleOrchestrator
{
    Task<bool> ProcessAchievementAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default);
    Task<bool> ProcessAchievementSilentlyAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default);
    Task ResetAchievementAsync(ulong guildId, AchievementRole achievement, Func<int, int, Task> progress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Orchestrates the processing and management of achievement roles within a guild.
/// </summary>
/// <remarks>This class coordinates the interaction between various services to handle achievements, including
/// checking for existing achievements, assigning roles, and sending notifications. It is designed to work
/// asynchronously and can handle operations such as processing individual achievements and resetting achievements for a
/// guild.</remarks>
public class AchievementRoleOrchestrator(
    IAchievementService achievementService,
    IAchievementRoleService achievementRoleService,
    IAchievementMessageService achievementMessageService,
    IDiscordGatewayClient gatewayClient) : IAchievementRoleOrchestrator
{
    public async Task<bool> ProcessAchievementAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (await achievementService.HasAchievementAsync(mhid, achievement, cancellationToken))
        {
            var role = EnumUtils.ToRole(achievement);
            await achievementRoleService.AddRoleAsync(userId, guildId, role, cancellationToken);
            await achievementMessageService.SendAchievementMessageAsync(userId, guildId, achievement, cancellationToken);

            return true;
        }

        return false;
    }

    public async Task<bool> ProcessAchievementSilentlyAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (await achievementService.HasAchievementAsync(mhid, achievement, cancellationToken))
        {
            var role = EnumUtils.ToRole(achievement);
            await achievementRoleService.AddRoleAsync(userId, guildId, role, cancellationToken);
            // No message sent for silent processing
            return true;
        }
        return false;
    }

    public async Task ResetAchievementAsync(ulong guildId, AchievementRole achievement, Func<int, int, Task> progress, CancellationToken cancellationToken = default)
    {
        var role = EnumUtils.ToRole(achievement);
        var roleId = await achievementRoleService.GetRoleIdAsync(guildId, role)
            ?? throw new InvalidOperationException($"The role for {achievement} has not been configured yet. An admin needs to use `/config role`.");

        GuildUser[] usersWithAchievement = [..gatewayClient.Cache.Guilds[guildId]?.Users.Values
            .Where(u => u.RoleIds.Contains(roleId)) ?? []];

        await progress(0, usersWithAchievement.Length);

        for (int i = 0; i < usersWithAchievement.Length; i++)
        {
            // Add achiever role first, since it may fail if not configured. That way we still have the achievement role set on the user.
            await achievementRoleService.AddRoleAsync(usersWithAchievement[i].Id, guildId, Role.Achiever, cancellationToken: default);
            await achievementRoleService.RemoveRoleAsync(usersWithAchievement[i].Id, guildId, role, cancellationToken: default);

            if (i % 10 == 0)
            {
                await progress(i + 1, usersWithAchievement.Length);
            }
        }
    }
}
