using BouncerBot.Db;
using BouncerBot.Db.Models;

using Microsoft.EntityFrameworkCore;

namespace BouncerBot.Modules.Bounce;

public class BounceService(BouncerBotDbContext dbContext) : IBounceService
{
    public const int ResultsPerPage = 10;

    public async Task<bool> IsHunterBannedAsync(uint mouseHuntId, ulong guildId)
    {
        return await dbContext.BannedHunters
            .AnyAsync(bh => bh.MouseHuntId == mouseHuntId && bh.GuildId == guildId);
    }

    public async Task<BannedHunter?> GetBannedHunterAsync(uint mouseHuntId, ulong guildId)
    {
        return await dbContext.BannedHunters
            .FirstOrDefaultAsync(bh => bh.MouseHuntId == mouseHuntId && bh.GuildId == guildId);
    }

    public async Task<IEnumerable<BannedHunter>> ListBannedHuntersAsync(ulong guildId, int page)
    {
        return await dbContext.BannedHunters
            .Where(bh => bh.GuildId == guildId)
            .OrderBy(bh => bh.MouseHuntId)
            .Skip(page * ResultsPerPage)
            .Take(ResultsPerPage + 1)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddBannedHunterAsync(uint mouseHuntId, ulong guildId, string? note = null)
    {
        var bannedHunter = new BannedHunter
        {
            MouseHuntId = mouseHuntId,
            GuildId = guildId,
            Note = note
        };

        dbContext.BannedHunters.Add(bannedHunter);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveBannedHunterAsync(uint mouseHuntId, ulong guildId)
    {
        var bannedHunter = await GetBannedHunterAsync(mouseHuntId, guildId);
        if (bannedHunter != null)
        {
            dbContext.BannedHunters.Remove(bannedHunter);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateBannedHunterNoteAsync(uint mouseHuntId, ulong guildId, string? note)
    {
        var bannedHunter = await GetBannedHunterAsync(mouseHuntId, guildId);
        if (bannedHunter != null)
        {
            bannedHunter.Note = note;
            await dbContext.SaveChangesAsync();
        }
    }
}
