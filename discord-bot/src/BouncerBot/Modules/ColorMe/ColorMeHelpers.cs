using BouncerBot.Services;
using NetCord;

namespace BouncerBot.Modules.ColorRole;

internal static class ColorMeHelpers
{
    public static GuildUser GetGuildUser(IDiscordGatewayClient gatewayClient, ulong guildId, ulong userId)
    {
        if (!gatewayClient.Cache.Guilds.TryGetValue(guildId, out var guild))
        {
            throw new InvalidOperationException("I was unable to find the server in my cache.");
        }

        if (!guild.Users.TryGetValue(userId, out var user))
        {
            throw new InvalidOperationException("I was unable to find the user in my cache.");
        }

        return user;
    }

    public static IReadOnlyList<SelectableColorMeRole> GetAvailableColors(
        GuildUser user,
        IEnumerable<SelectableColorMeRole> colors,
        IReadOnlyDictionary<PowerType, ulong> achievementRoleIds)
        => [.. colors
            .Where(color
                => color.PowerType is null
                    || achievementRoleIds.TryGetValue(color.PowerType.Value, out var roleId)
                    && user.RoleIds.Contains(roleId))];
                    /*
                    && true )];
                    */

    public static async Task<IReadOnlyDictionary<PowerType, ulong>> GetAchievementRoleIdsAsync(
        ulong guildId,
        IEnumerable<SelectableColorMeRole> colors,
        IRoleService roleService)
    {
        var roleIds = new Dictionary<PowerType, ulong>();
        foreach (var powerType in colors
            .Where(color => color.PowerType is not null)
            .Select(color => color.PowerType!.Value)
            .Distinct())
        {
            var role = Enum.Parse<Role>($"{powerType}Master");
            try
            {
                var roleId = await roleService.GetRoleIdAsync(guildId, role);
                if (roleId is > 0)
                {
                    roleIds[powerType] = roleId.Value;
                }
            }
            catch (RoleNotConfiguredException)
            {
                // An unconfigured achievement cannot verify access to its color.
            }
        }

        return roleIds;
    }

    public static IReadOnlyList<ulong> GetManagedRoleIds(IEnumerable<SelectableColorMeRole> colors)
        => colors.Select(color => color.RoleId).Distinct().ToArray();
}
