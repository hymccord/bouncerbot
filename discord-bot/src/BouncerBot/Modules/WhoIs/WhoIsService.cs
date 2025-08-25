using BouncerBot.Db;
using BouncerBot.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace BouncerBot.Modules.WhoIs;

public interface IWhoIsService
{
    Task<VerifiedUser?> GetVerifiedUserByDiscordIdAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<VerifiedUser?> GetVerifiedUserByMouseHuntIdAsync(ulong guildId, uint mouseHuntId, CancellationToken cancellationToken = default);
}

public class WhoIsService(BouncerBotDbContext dbContext) : IWhoIsService
{
    public async Task<VerifiedUser?> GetVerifiedUserByDiscordIdAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        return await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.GuildId == guildId && vu.DiscordId == discordId, cancellationToken);
    }

    public async Task<VerifiedUser?> GetVerifiedUserByMouseHuntIdAsync(ulong guildId, uint mouseHuntId, CancellationToken cancellationToken = default)
    {
        return await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.GuildId == guildId && vu.MouseHuntId == mouseHuntId, cancellationToken);
    }
}
