namespace BouncerBot.Modules.Achieve;

public class AchievementRoleOrchestrator(AchievementService achievementService, AchievementRoleService achievementRoleService, AchievementMessageService achievementMessageService)
{
    public async Task<bool> ProcessAchievementAsync(uint mhid, ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (await achievementService.HasAchievementAsync(mhid, achievement, cancellationToken))
        {
            await achievementRoleService.GrantRoleAsync(userId, guildId, achievement, cancellationToken);
            await achievementMessageService.SendAchievementMessageAsync(userId, guildId, achievement, cancellationToken);

            return true;
        }

        return false;
    }
}
