namespace BouncerBot.Db.Models;
public class RoleSetting
{
    public ulong GuildId { get; set; }

    public required Role Role { get; set; }

    public required ulong DiscordRoleId { get; set; }
}
