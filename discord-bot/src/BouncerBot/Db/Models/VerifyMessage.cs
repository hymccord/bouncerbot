namespace BouncerBot.Db.Models;

public class VerifyMessage
{
    public int Id { get; init; }

    public ulong MessageId { get; set; }
    public ulong ChannelId { get; init; }
}
