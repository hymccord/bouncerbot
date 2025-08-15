using BouncerBot.Db;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace BouncerBot.Services;

public interface IGuildLoggingService
{
    Task<RestMessage?> LogAsync(ulong guildId, LogType logType, MessageProperties message, CancellationToken cancellationToken = default);
    Task LogAchievementAsync(ulong guildId, AchievementRole achievement, string content, CancellationToken cancellationToken = default);
    //Task<RestMessage?> LogAsync(ulong guildId, LogType logType, string content, CancellationToken cancellationToken = default);
    //Task<RestMessage?> LogAsync(ulong guildId, LogType logType, EmbedProperties embedProperties, CancellationToken cancellationToken = default);
    //Task<RestMessage?> LogAsync(ulong guildId, LogType logType, MessageProperties message, CancellationToken cancellationToken = default)}
}

public enum LogType
{
    General,
    Verification,
    Achievement,
    EggMaster
}

internal class GuildLoggingService(
    ILogger<GuildLoggingService> logger,
    BouncerBotDbContext dbContext,
    IDiscordGatewayClient gatewayClient) : IGuildLoggingService
{
    public async Task<RestMessage?> LogAsync(ulong guildId, LogType logType, MessageProperties message, CancellationToken cancellationToken = default)
    {
        // no guild, no log
        if (!gatewayClient.Cache.Guilds.TryGetValue(guildId, out var guild))
        {
            return null;
        }

        var logSetting = await dbContext.LogSettings.FindAsync([guildId], cancellationToken: cancellationToken);

        if (logSetting is null)
        {
            return null;
        }

        var logChannelId = logType switch
        {
            LogType.General => logSetting.GeneralId,
            LogType.Verification => logSetting.VerificationId,
            LogType.Achievement => logSetting.AchievementId,
            _ => logSetting.GeneralId
        } ?? logSetting.GeneralId ?? 0;

        if (guild.Channels.TryGetValue(logChannelId, out var guildChannel) && guildChannel is TextChannel logChannel)
        {
            return await logChannel.SendMessageAsync(message, cancellationToken: cancellationToken);
        }

        return null;
    }

    public async Task LogAchievementAsync(ulong guildId, AchievementRole achievement, string content, CancellationToken cancellationToken = default)
    {
        // no guild, no log
        if (!gatewayClient.Cache.Guilds.TryGetValue(guildId, out var guild))
        {
            return;
        }

        var logSetting = await dbContext.LogSettings.FindAsync(new object?[] { guildId }, cancellationToken: cancellationToken);
        if (logSetting is null)
        {
            return;
        }

        var logChannelId = logSetting.AchievementId;
        var achievementOverride = await dbContext.AchievementLogOverrides
            .FirstOrDefaultAsync(a => a.GuildId == guildId && a.AchievementRole == achievement, cancellationToken: cancellationToken);
        if (achievementOverride?.ChannelId is not null)
        {
            logChannelId = achievementOverride.ChannelId;
        }

        if (logChannelId is null)
        {
            logger.LogWarning("No log channel configured for achievement logging in guild {GuildId}. Trying to send '{Message}'", guildId, content);

            return;
        }

        guild.Channels.TryGetValue(logChannelId.Value, out var guildChannel);
        if (guildChannel is null)
        {
            logger.LogWarning("Log channel {LogChannelId} not found in guild {GuildId}. Trying to send '{Message}'", logChannelId.Value, guildId, content);
            return;
        }

        if (guildChannel is not TextChannel textChannel)
        {
            logger.LogWarning("Log channel {LogChannelId} in guild {GuildId} is not a text channel. Trying to send '{Message}'", logChannelId.Value, guildId, content);
            return;
        }

        logger.LogInformation("Logging achievement '{Achievement}' to channel {LogChannelId} in guild {GuildId}: {Message}", achievement, logChannelId.Value, guildId, content);
        await textChannel.SendMessageAsync(new MessageProperties
        {
            Content = content
        }, cancellationToken: cancellationToken);
    }
}
