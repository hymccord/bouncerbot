using BouncerBot.Db.Models;

namespace BouncerBot.Modules.Bounce;

public interface IBounceService
{
    Task<bool> IsHunterBannedAsync(uint mouseHuntId, ulong guildId);
    Task<BannedHunter?> GetBannedHunterAsync(uint mouseHuntId, ulong guildId);
    Task<IEnumerable<BannedHunter>> ListBannedHuntersAsync(ulong guildId, int page);
    Task AddBannedHunterAsync(uint mouseHuntId, ulong guildId, string? note = null);
    Task RemoveBannedHunterAsync(uint mouseHuntId, ulong guildId);
    Task UpdateBannedHunterNoteAsync(uint mouseHuntId, ulong guildId, string? note);
}
