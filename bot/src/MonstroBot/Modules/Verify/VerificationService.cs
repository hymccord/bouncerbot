using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MonstroBot.Db;

namespace MonstroBot.Modules.Verify;
public class VerificationService(ILogger<VerificationService> logger, MonstroBotDbContext dbContext)
{
    public async Task AddVerifiedUserAsync(uint mouseHuntId, ulong guildId, ulong discordId)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId);
        if (existingUser is null)
        {
            await dbContext.VerifiedUsers.AddAsync(new Db.Models.VerifiedUser
            {
                MouseHuntId = mouseHuntId,
                GuildId = guildId,
                DiscordId = discordId
            });

            await dbContext.SaveChangesAsync();
        }
        else
        {
            logger.LogInformation("User {UserId} is already verified in guild {GuildId}", discordId, guildId);
        }
    }

    public async Task<bool> IsDiscordUserVerifiedAsync(ulong guildId, ulong discordId)
        => await dbContext.VerifiedUsers
            .AnyAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId);

    public async Task RemoveVerifiedUser(ulong guildId, ulong discordId)
    {
        var existingUser = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == discordId && vu.GuildId == guildId);
        if (existingUser is not null)
        {
            dbContext.VerifiedUsers.Remove(existingUser);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            logger.LogInformation("User {UserId} is not verified in guild {GuildId}", discordId, guildId);
        }
    }
}
