namespace BouncerBot.Db.Models;
public class AchievementMessage
{
    public required ulong GuildId { get; set; }

    public required AchievementRole AchievementRole { get; set; }

    public required string Message { get; set; }
}
