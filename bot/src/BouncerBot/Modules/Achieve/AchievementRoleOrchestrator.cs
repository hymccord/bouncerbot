
using NetCord;
using NetCord.Gateway;

namespace BouncerBot.Modules.Achieve;

public class AchievementRoleOrchestrator(
    AchievementService achievementService,
    AchievementRoleService achievementRoleService,
    AchievementMessageService achievementMessageService,
    GatewayClient gatewayClient)
{
    public async Task<bool> ProcessAchievementAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (await achievementService.HasAchievementAsync(mhid, achievement, cancellationToken))
        {
            await achievementRoleService.AddRoleAsync(userId, guildId, achievement, cancellationToken);
            await achievementMessageService.SendAchievementMessageAsync(userId, guildId, achievement, cancellationToken);

            return true;
        }

        return false;
    }

    internal async Task ResetAchievementsAsync(ulong guildId, AchievementRole achievement, Func<int, int, Task> progress, CancellationToken cancellationToken = default)
    {
        var roleId = await achievementRoleService.GetRoleIdAsync(guildId, achievement)
            ?? throw new InvalidOperationException($"The role for {achievement} has not been configured yet. An admin needs to use `/config role`.");

        GuildUser[] usersWithAchievement = [..gatewayClient.Cache.Guilds[guildId]?.Users.Values
            .Where(u => u.RoleIds.Contains(roleId)) ?? []];

        await progress(0, usersWithAchievement.Length);

        for (int i = 0; i < usersWithAchievement.Length; i++)
        {
            await achievementRoleService.ResetRoleAndAddAchieverAsync(usersWithAchievement[i].Id, guildId, achievement, cancellationToken: default);

            if (i % 10 == 0)
            {
                await progress(i + 1, usersWithAchievement.Length);
            }
        }
    }
}
