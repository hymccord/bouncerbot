using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using BouncerBot.Db;
using BouncerBot.Services;

using NetCord.Rest;

namespace BouncerBot.Modules.Verify;
public class VerificationService(ILogger<VerificationService> logger, BouncerBotDbContext dbContext, IGuildLoggingService guildLoggingService, RestClient restClient)
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

            await guildLoggingService.LogAsync(guildId, LogType.Verification, new()
            {
                Content = $"<@{discordId}> is hunter {mouseHuntId} <https://p.mshnt.ca/{mouseHuntId}>",
                AllowedMentions = AllowedMentionsProperties.None,
            });
        }
        else
        {
            logger.LogInformation("User {UserId} is already verified in guild {GuildId}", discordId, guildId);
        }

        var roleConfig = await dbContext.RoleSettings.FindAsync(guildId);
        if (roleConfig?.VerifiedId is ulong roleId)
        {
            _ = restClient.AddGuildUserRoleAsync(guildId, discordId, roleId);
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

        var roleConfig = await dbContext.RoleSettings.FindAsync(guildId);
        if (roleConfig?.VerifiedId is ulong roleId)
        {
            _ = restClient.RemoveGuildUserRoleAsync(guildId, discordId, roleId);
        }
    }
}
