using NetCord;

namespace BouncerBot.Modules.ColorRole;

internal static class ColorRoleHelpers
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

    public static bool HasBaseAccess(GuildUser user, ColorRoleOptions options)
        => options.BaseAccessRoleIds.Any(user.RoleIds.Contains);

    public static IReadOnlyList<SelectableColorRole> GetAvailableColors(GuildUser user, ColorRoleOptions options)
    {
        var baseColors = HasBaseAccess(user, options)
            ? options.BaseColors.Where(IsConfiguredColor)
            : Enumerable.Empty<SelectableColorRole>();

        // Each fancy color is unlocked individually by its own required roles,
        // e.g. a mastery role unlocks only that mastery's color.
        var fancyColors = options.FancyColors
            .Where(IsConfiguredColor)
            .Where(color => color.RequiredRoleIds.Any(user.RoleIds.Contains));

        return baseColors
            .Concat(fancyColors)
            .DistinctBy(color => color.RoleId)
            .ToArray();
    }

    public static IReadOnlyList<ulong> GetManagedRoleIds(ColorRoleOptions options)
        => options.BaseColors
            .Concat<SelectableColorRole>(options.FancyColors)
            .Where(IsConfiguredColor)
            .Select(color => color.RoleId)
            .Distinct()
            .ToArray();

    private static bool IsConfiguredColor(SelectableColorRole color)
        => color.RoleId != 0 && !string.IsNullOrWhiteSpace(color.Name);
}
