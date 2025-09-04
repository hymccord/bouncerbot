using NetCord;

namespace BouncerBot.Attributes;

/// <summary>
/// Represents a guild slash command attribute that requires the "Manage Roles" permission.
/// </summary>
/// <remarks>This attribute is used to define a slash command that is restricted to guild contexts and requires
/// the user to have the "Manage Roles" permission. It is intended for commands that involve role management
/// functionality within a guild.</remarks>
internal class ManageRolesSlashCommandAttribute : BouncerBotSlashCommandAttribute
{
    public ManageRolesSlashCommandAttribute(string name, string description)
        : base(name, description)
    {
        DefaultGuildPermissions = Permissions.ManageRoles;
    }
}
