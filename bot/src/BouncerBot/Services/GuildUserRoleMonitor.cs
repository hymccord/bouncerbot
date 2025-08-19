using BouncerBot.Db;
using BouncerBot.Modules.Verification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Rest;

namespace BouncerBot.Services;

public interface IGuildUserRoleMonitorService
{
    Task HandleRolesAddedAsync(GuildUser guildUser, IEnumerable<ulong> addedRoles);
    Task HandleRolesRemovedAsync(GuildUser guildUser, IEnumerable<ulong> removedRoles);
}

public class GuildUserRoleMonitor(
    ILogger<GuildUserRoleMonitor> logger,
    IGuildLoggingService guildLoggingService,
    IVerificationService verificationService,
    IVerificationOrchestrator verificationOrchestrator,
    BouncerBotDbContext dbContext) : IGuildUserRoleMonitorService
{
    public async Task HandleRolesAddedAsync(GuildUser guildUser, IEnumerable<ulong> addedRoles)
    {
        if (!addedRoles.Any())
        {
            return;
        }

        if ((await dbContext.RoleSettings.FirstOrDefaultAsync(rs => rs.GuildId == guildUser.GuildId && rs.Role == Role.Verified)) is { DiscordRoleId: var verifiedRoleId }
            && verifiedRoleId > 0
            && addedRoles.Contains(verifiedRoleId))
        {
            await HandleVerifiedAddedAsync(guildUser, verifiedRoleId);
        }
    }

    public async Task HandleRolesRemovedAsync(GuildUser guildUser, IEnumerable<ulong> removedRoles)
    {
        if (!removedRoles.Any())
        {
            return;
        }

        if ((await dbContext.RoleSettings.FirstOrDefaultAsync(rs => rs.GuildId == guildUser.GuildId && rs.Role == Role.Verified)) is {DiscordRoleId: var verifiedRoleId}
            && verifiedRoleId > 0
            && removedRoles.Contains(verifiedRoleId))
        {
            await HandleVerifiedRemovedAsync(guildUser);
        }
    }

    private async Task HandleVerifiedAddedAsync(GuildUser guildUser, ulong verifiedRoleId)
    {
        if (!await verificationService.IsDiscordUserVerifiedAsync(guildUser.GuildId, guildUser.Id))
        {
            logger.LogInformation("Removing invalid verified role from Discord User {DiscordUserId} in Guild {GuildId}", guildUser.Id, guildUser.GuildId);

            await guildUser.RemoveRoleAsync(verifiedRoleId);
            await guildLoggingService.LogAsync(guildUser.GuildId, LogType.General, new MessageProperties
            {
                Content = $"<@&{verifiedRoleId}> role added to unverified user <@{guildUser.Id}>. Removing role.",
                AllowedMentions = AllowedMentionsProperties.None,
            });
        }
    }

    private async Task HandleVerifiedRemovedAsync(GuildUser guildUser)
    {
        await verificationOrchestrator.ProcessVerificationAsync(
            VerificationType.Remove,
            new VerificationParameters
            {
                GuildId = guildUser.GuildId,
                DiscordUserId = guildUser.Id,
            });
    }
}
