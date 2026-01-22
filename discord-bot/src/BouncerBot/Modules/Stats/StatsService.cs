using BouncerBot.Services;

namespace BouncerBot.Modules.Stats;
public interface IStatsService
{
    Task<Dictionary<Role, int>> GetGuildStatsAsync(ulong guildId);
}

internal class StatsService(
    IRoleService roleService
) : IStatsService
{
    public async Task<Dictionary<Role, int>> GetGuildStatsAsync(ulong guildId)
    {
        // iterate over AchievementRole enum values
        var result = new Dictionary<Role, int>();
        foreach (var role in Enum.GetValues<Role>())
        {
            var count = await roleService.GetRoleUserCount(guildId, role);
            result[role] = count;
        }

        return result;
    }
}
