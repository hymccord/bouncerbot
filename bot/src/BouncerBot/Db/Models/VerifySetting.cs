namespace BouncerBot.Db.Models;
public class VerifySetting
{
    public ulong GuildId { get; set; }
    public Rank MinimumRank { get; set; } = Rank.Novice;
}
