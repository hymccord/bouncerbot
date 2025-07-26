using System.Security.Cryptography;
using System.Text;

namespace BouncerBot.Db.Models;

public class VerificationHistory
{
    public ulong GuildId { get; init; }
    public ulong DiscordId { get; init; }
    public string MouseHuntIdHash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Creates a hash of the MouseHunt ID 
    /// </summary>
    public static string HashMouseHuntId(uint hunterId)
    {
        var input = $"{hunterId}";
        var bytes = SHA512.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Verifies if a MouseHunt ID matches the stored hash
    /// </summary>
    public bool VerifyMouseHuntId(uint hunterId)
    {
        var hash = HashMouseHuntId(hunterId);
        return hash.Equals(MouseHuntIdHash);
    }
}
