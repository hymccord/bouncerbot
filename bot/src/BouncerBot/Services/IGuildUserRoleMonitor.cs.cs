using BouncerBot.Db;
using BouncerBot.Modules.Verification;

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

        var roleSettings = await dbContext.RoleSettings.FindAsync(guildUser.GuildId);
        if (roleSettings is null)
        {
            return;
        }

        if (roleSettings.VerifiedId is ulong verifiedRoleId && verifiedRoleId > 0 && addedRoles.Contains(roleSettings.VerifiedId ?? 0))
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

        var roleSettings = await dbContext.RoleSettings.FindAsync(guildUser.GuildId);
        if (roleSettings is null)
        {
            return;
        }

        if (roleSettings.VerifiedId is ulong verifiedRoleId && verifiedRoleId > 0 && removedRoles.Contains(verifiedRoleId))
        {
            await HandleVerifiedRemovedAsync(guildUser, verifiedRoleId);
        }
    }

    private async Task HandleVerifiedAddedAsync(GuildUser guildUser, ulong verifiedRoleId)
    {
        if (!await verificationService.IsDiscordUserVerifiedAsync(guildUser.GuildId, guildUser.Id))
        {
            logger.LogWarning("User {UserId} in guild {GuildId} was added to the verified role but is not verified. Role will be removed.", guildUser.Id, guildUser.GuildId);

            await guildUser.RemoveRoleAsync(verifiedRoleId);
            await guildLoggingService.LogAsync(guildUser.GuildId, LogType.General, new MessageProperties
            {
                Content = $"Role <@&{verifiedRoleId}> was added to <@{guildUser.Id}> ({guildUser.Id}) but is not verified. Role will be removed.",
                AllowedMentions = AllowedMentionsProperties.None,
            });
        }
    }

    private async Task HandleVerifiedRemovedAsync(GuildUser guildUser, ulong verifiedRoleId)
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
