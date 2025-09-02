using BouncerBot.Services;

using NetCord;

namespace BouncerBot.Modules.Achieve;

public interface IAchievementRoleOrchestrator
{
    Task<ClaimResult> GrantAchievementAsync(ulong userId, ulong guildId, AchievementRole achievement, NotificationMode notificationMode, CancellationToken cancellationToken = default);
    Task<ClaimResult> ProcessAchievementAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default);
    Task<ClaimResult> ProcessAchievementSilentlyAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default);
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
    IRoleService roleService,
    IAchievementMessageService achievementMessageService,
    IDiscordGatewayClient gatewayClient) : IAchievementRoleOrchestrator
{
    public async Task<ClaimResult> ProcessAchievementAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (!await achievementService.HasAchievementAsync(mhid, achievement, cancellationToken))
        {
            return ClaimResult.NotAchieved;
        }

        return await AssignRoleIfNotHasAsync(userId, guildId, achievement, notification: NotificationMode.SendMessage, cancellationToken);
    }

    public async Task<ClaimResult> GrantAchievementAsync(ulong userId, ulong guildId, AchievementRole achievement, NotificationMode notificationMode, CancellationToken cancellationToken = default)
    {
        return await AssignRoleIfNotHasAsync(userId, guildId, achievement, notificationMode, cancellationToken);
    }

    public async Task<ClaimResult> ProcessAchievementSilentlyAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (!await achievementService.HasAchievementAsync(mhid, achievement, cancellationToken))
        {
            return ClaimResult.NotAchieved;
        }

        return await AssignRoleIfNotHasAsync(userId, guildId, achievement, notification: NotificationMode.Silent, cancellationToken);
    }

    public async Task ResetAchievementAsync(ulong guildId, AchievementRole achievement, Func<int, int, Task> progress, CancellationToken cancellationToken = default)
    {
        var role = EnumUtils.ToRole(achievement);
        var roleId = await roleService.GetRoleIdAsync(guildId, role)
            ?? throw new RoleNotConfiguredException(role);

        ulong[] usersWithAchievement = [..gatewayClient.Cache.Guilds[guildId]?.Users.Values
            .Where(u => u.RoleIds.Contains(roleId)).Select(u => u.Id) ?? []];

        await progress(0, usersWithAchievement.Length);

        for (var i = 0; i < usersWithAchievement.Length; i++)
        {
            // Add achiever role first, since it may fail if not configured. That way we still have the achievement role set on the user.
            await roleService.AddRoleAsync(usersWithAchievement[i], guildId, Role.Achiever, cancellationToken: default);
            await roleService.RemoveRoleAsync(usersWithAchievement[i], guildId, role, cancellationToken: default);

            if (i % 10 == 0)
            {
                await progress(i + 1, usersWithAchievement.Length);
            }
        }
    }

    private async Task<ClaimResult> AssignRoleIfNotHasAsync(ulong userId, ulong guildId, AchievementRole achievement, NotificationMode notification, CancellationToken cancellationToken)
    {
        var role = EnumUtils.ToRole(achievement);

        if (await roleService.HasRoleAsync(userId, guildId, role))
        {
            return ClaimResult.AlreadyHasRole;
        }

        await roleService.AddRoleAsync(userId, guildId, role, cancellationToken);

        if (notification == NotificationMode.SendMessage)
        {
            await achievementMessageService.SendAchievementMessageAsync(userId, guildId, achievement, cancellationToken);
        }

        return ClaimResult.Success;
    }
}

public enum ClaimResult
{
    Success,
    AlreadyHasRole,
    NotAchieved,
}

public enum NotificationMode
{
    Silent,
    SendMessage,
    // Future: SendDM, LogOnly, etc.
}
