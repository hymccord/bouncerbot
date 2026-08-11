using NetCord;
using NetCord.Gateway;
using DiscordRole = NetCord.Role;

namespace BouncerBot.Modules.ColorMe;

public interface IColorRoleRegistry
{
    IReadOnlyList<SelectableColorMeRole> GetRoles(ulong guildId);
    void UpdateGuild(Guild guild);
    void UpdateRole(DiscordRole role);
    void RemoveRole(RoleDeleteEventArgs role);
}

public sealed class ColorMeRoleRegistry : IColorRoleRegistry
{
    private const string RolePrefix = "ColorMe";
    private readonly Lock _lock = new();
    private readonly Dictionary<ulong, Dictionary<ulong, SelectableColorMeRole>> _roles = [];

    public IReadOnlyList<SelectableColorMeRole> GetRoles(ulong guildId)
    {
        lock (_lock)
        {
            return _roles.TryGetValue(guildId, out var roles)
                ? roles.Values.OrderBy(role => role.Name).ToArray()
                : [];
        }
    }

    public void UpdateGuild(Guild guild)
    {
        lock (_lock)
        {
            _roles[guild.Id] = guild.Roles.Values
                .Select(CreateColorRole)
                .OfType<SelectableColorMeRole>()
                .ToDictionary(role => role.RoleId);
        }
    }

    public void UpdateRole(DiscordRole role)
    {
        lock (_lock)
        {
            if (!_roles.TryGetValue(role.GuildId, out var roles))
            {
                roles = [];
                _roles[role.GuildId] = roles;
            }

            var colorRole = CreateColorRole(role);
            if (colorRole is null)
            {
                roles.Remove(role.Id);
            }
            else
            {
                roles[role.Id] = colorRole;
            }
        }
    }

    public void RemoveRole(RoleDeleteEventArgs role)
    {
        lock (_lock)
        {
            if (_roles.TryGetValue(role.GuildId, out var roles))
            {
                roles.Remove(role.RoleId);
            }
        }
    }

    private static SelectableColorMeRole? CreateColorRole(DiscordRole role)
    {
        if (!role.Name.StartsWith(RolePrefix, StringComparison.Ordinal) || role.Name.Length == RolePrefix.Length)
        {
            return null;
        }

        var name = role.Name[RolePrefix.Length..];
        var isPowerType = Enum.TryParse<PowerType>(name, ignoreCase: true, out var powerType)
            && powerType != PowerType.None;

        return new SelectableColorMeRole(name, role.Id, isPowerType ? powerType : null);
    }
}
