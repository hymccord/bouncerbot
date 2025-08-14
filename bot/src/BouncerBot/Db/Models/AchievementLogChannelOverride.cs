namespace BouncerBot.Db.Models;

public class AchievementLogChannelOverride
{
    public ulong GuildId { get; set; }
    public AchievementRole AchievementRole { get; set; }
    public ulong ChannelId { get; set; }
}
