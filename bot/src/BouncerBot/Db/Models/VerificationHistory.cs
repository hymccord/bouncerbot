using System.Security.Cryptography;

namespace BouncerBot.Db.Models;

public class VerificationHistory
{
    public ulong GuildId { get; init; }
    public string DiscordIdHash { get; init; } = string.Empty;
    public string MouseHuntIdHash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public static string HashValue(ulong value)
    {
        var bytes = SHA512.HashData(BitConverter.GetBytes(value));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Verifies if a MouseHunt ID matches the stored hash
    /// </summary>
    public bool VerifyMouseHuntId(uint hunterId)
    {
        var hash = HashValue(hunterId);
        return hash.Equals(MouseHuntIdHash);
    }

    public bool VerifyDiscordId(ulong discordId)
    {
        var hash = HashValue(discordId);
        return hash.Equals(DiscordIdHash);
    }
}
