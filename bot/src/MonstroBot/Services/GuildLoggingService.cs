using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonstroBot.Db;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace MonstroBot.Services;

public interface IGuildLoggingService
{
    Task LogAsync(ulong guildId, LogType logType, MessageProperties message);
}

public enum LogType
{
    General,
    Verification,
    Achievement,
    EggMaster
}

internal class GuildLoggingService(ILogger<GuildLoggingService> logger, IDbContextFactory<MonstroBotDbContext> dbContextFactory, GatewayClient gatewayClient) : IGuildLoggingService
{
    public async Task LogAsync(ulong guildId, LogType logType, MessageProperties message)
    {
        // no guild, no log
        if (!gatewayClient.Cache.Guilds.TryGetValue(guildId, out Guild? guild))
        {
            return;
        }

        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var logSetting = await dbContext.LogSettings.FindAsync(guildId);

        if (logSetting is null)
        {
            return;
        }

        ulong logChannelId = logType switch
        {
            LogType.General => logSetting.LogId,
            LogType.Verification => logSetting.VerificationId,
            LogType.Achievement => logSetting.FlexId,
            LogType.EggMaster => logSetting.EggMasterId,
            _ => logSetting.LogId
        } ?? 0;

        if (guild.Channels.TryGetValue(logChannelId, out IGuildChannel? guildChannel) && guildChannel is TextChannel logChannel)
        {
            _ = Task.Run(async () =>
            {
                await logChannel.SendMessageAsync(message);
            });
        }
    }
}
