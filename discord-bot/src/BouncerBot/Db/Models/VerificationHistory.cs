using System.Security.Cryptography;

namespace BouncerBot.Db.Models;

public class VerificationHistory
{
    public ulong GuildId { get; init; }
    public string DiscordIdHash { get; init; } = string.Empty;
    public string MouseHuntIdHash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
