namespace BouncerBot.Db.Models;

public class VerifyMessage
{
    public int Id { get; init; }

    public ulong MessageId { get; init; }
    public ulong ChannelId { get; init; }
}
