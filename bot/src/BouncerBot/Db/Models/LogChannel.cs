namespace BouncerBot.Db.Models;
public class LogChannelsSetting
{
    public ulong GuildId { get; set; }
    public ulong? GeneralId { get; set; }
    public ulong? AchievementId { get; set; }
    public ulong? VerificationId { get; set; }
}
