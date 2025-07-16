using BouncerBot.Db;

using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Rest;

namespace BouncerBot.Services;

public interface IGuildUserRoleMonitorService
{
    Task HandleRolesAddedAsync(GuildUser guildUser, IEnumerable<ulong> addedRoles);
    Task HandleRolesRemovedAsync(GuildUser guildUser, IEnumerable<ulong> removedRoles);
}

public class GuildUserRoleMonitor(ILogger<GuildUserRoleMonitor> logger, IGuildLoggingService guildLoggingService, BouncerBotDbContext dbContext) : IGuildUserRoleMonitorService
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

        if (addedRoles.Contains(roleSettings.TradeBannedId ?? 0))
        {
            await guildLoggingService.LogAsync(guildUser.GuildId, LogType.General, new MessageProperties
            {
                Content = $"Role <@&{roleSettings.TradeBannedId}> added to <@{guildUser.Id}>.",
                AllowedMentions = AllowedMentionsProperties.None
            });
        }

        if (addedRoles.Contains(roleSettings.MapBannedId ?? 0))
        {
            await guildLoggingService.LogAsync(guildUser.GuildId, LogType.General, new MessageProperties
            {
                Content = $"Role <@&{roleSettings.MapBannedId}> added to <@{guildUser.Id}>.",
                AllowedMentions = AllowedMentionsProperties.None
            });
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

        if (removedRoles.Contains(roleSettings.TradeBannedId ?? 0))
        {
            await guildLoggingService.LogAsync(guildUser.GuildId, LogType.General, new MessageProperties
            {
                Content = $"Role <@&{roleSettings.TradeBannedId}> removed from <@{guildUser.Id}>.",
                AllowedMentions = AllowedMentionsProperties.None
            });
        }

        if (removedRoles.Contains(roleSettings.MapBannedId ?? 0))
        {
            await guildLoggingService.LogAsync(guildUser.GuildId, LogType.General, new MessageProperties
            {
                Content = $"Role <@&{roleSettings.MapBannedId}> removed from <@{guildUser.Id}>.",
                AllowedMentions = AllowedMentionsProperties.None
            });
        }
    }
}
