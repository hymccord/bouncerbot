namespace BouncerBot.Db.Models;

public class BannedHunter
{
    public ulong GuildId { get; init; }
    public uint MouseHuntId { get; init; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
