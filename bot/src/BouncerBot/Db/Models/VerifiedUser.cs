namespace BouncerBot.Db.Models;
public class VerifiedUser
{
    public ulong GuildId { get; init; }
    public ulong DiscordId { get; init; }
    public uint MouseHuntId { get; init; }

    public int? VerifyMessageId { get; init; }
    public VerifyMessage? VerifyMessage { get; set; }
}
