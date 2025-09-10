using System.Diagnostics.CodeAnalysis;
using BouncerBot.Db;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace BouncerBot.Services;

public interface IGuildLoggingService
{
    Task<LogMessageResult?> LogAsync(ulong guildId, LogType logType, MessageProperties message, CancellationToken cancellationToken = default);
    Task<LogMessageResult?> LogAchievementAsync(ulong guildId, AchievementRole achievement, string content, CancellationToken cancellationToken = default);
    Task LogErrorAsync(ulong guildId, string title, string content, CancellationToken cancellationToken = default);
    Task LogSuccessAsync(ulong guildId, string title, string content, CancellationToken cancellationToken = default);
    Task LogWarningAsync(ulong guildId, string title, string content, CancellationToken cancellationToken = default);
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
    IOptions<BouncerBotOptions> options,
    BouncerBotDbContext dbContext,
    IDiscordGatewayClient gatewayClient) : IGuildLoggingService
{
    public async Task<LogMessageResult?> LogAsync(ulong guildId, LogType logType, MessageProperties message, CancellationToken cancellationToken = default)
    {
        // no guild, no log
        if (!gatewayClient.Cache.Guilds.TryGetValue(guildId, out var guild))
        {
            return null;
        }

        var logSetting = await dbContext.LogSettings.FirstOrDefaultAsync(e => e.GuildId == guildId, cancellationToken: cancellationToken);
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

        if (!TryGetGuildChannel<TextChannel>(guild, logChannelId, out var guildChannel))
        {
            return null;
        }

        try
        {
            var restMessage = await guildChannel.SendMessageAsync(message, cancellationToken: cancellationToken);

            return new LogMessageResult
            {
                ChannelId = restMessage.ChannelId,
                MessageId = restMessage.Id
            };
        }
        catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            logger.LogInformation(ex, "Unable to log to channel {ChannelId} in guild {GuildId} due to permissions.", guildChannel.Id, guildId);
        }

        return null;
    }

    public async Task<LogMessageResult?> LogAchievementAsync(ulong guildId, AchievementRole achievement, string content, CancellationToken cancellationToken = default)
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

            return null;
        }

        if (!TryGetGuildChannel<TextChannel>(guild, logChannelId.Value, out var guildChannel))
        {
            logger.LogWarning("Log channel {LogChannelId} not found in guild {GuildId}. Trying to send '{Message}'", logChannelId.Value, guildId, content);
            return null;
        }

        logger.LogInformation("Logging achievement '{Achievement}' to channel {LogChannelId} in guild {GuildId}: {Message}", achievement, logChannelId.Value, guildId, content);

        try
        {
            var messageId = await guildChannel.SendMessageAsync(new MessageProperties
            {
                Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Success))
                        .AddComponents(
                        new TextDisplayProperties(content))
                    ],
                Flags = MessageFlags.IsComponentsV2,
                AllowedMentions = AllowedMentionsProperties.None,
            }, cancellationToken: cancellationToken);

            return new LogMessageResult
            {
                ChannelId = messageId.ChannelId,
                MessageId = messageId.Id
            };
        }
        catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            logger.LogWarning(ex, "Failed to log achievement '{Achievement}' to channel {LogChannelId} in guild {GuildId}: {Message}", achievement, logChannelId.Value, guildId, content);
        }

        return null;
    }

    public Task LogErrorAsync(ulong guildId, string title, string content, CancellationToken cancellationToken = default)
    {
        var message = CreateV2Embed(title, content, options.Value.Colors.Error);
        return LogAsync(guildId, LogType.General, message, cancellationToken);
    }

    public Task LogSuccessAsync(ulong guildId, string title, string content, CancellationToken cancellationToken = default)
    {
        var message = CreateV2Embed(title, content, options.Value.Colors.Success);
        return LogAsync(guildId, LogType.General, message, cancellationToken);
    }

    public Task LogWarningAsync(ulong guildId, string title, string content, CancellationToken cancellationToken = default)
    {
        var message = CreateV2Embed(title, content, options.Value.Colors.Warning);
        return LogAsync(guildId, LogType.General, message, cancellationToken);
    }

    private static MessageProperties CreateV2Embed(string title, string description, int color) => new()
    {
        Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new (color))
                    .AddComponents(
                        new TextDisplayProperties($"**{title}**"),
                        new ComponentSeparatorProperties().WithSpacing(ComponentSeparatorSpacingSize.Small).WithDivider(true),
                        new TextDisplayProperties(description)
                    )
                ],
        AllowedMentions = AllowedMentionsProperties.None,
        Flags = MessageFlags.IsComponentsV2
    };

    private static bool TryGetGuildChannel<T>(Guild guild, ulong channelId, [NotNullWhen(true)] out T? guildChannel)
    {
        if (guild.Channels.TryGetValue(channelId, out var channel) && channel is T typedChannel)
        {
            guildChannel = typedChannel;
            return true;
        }
        if (guild.ActiveThreads.TryGetValue(channelId, out var thread) && thread is T threadChannel)
        {
            guildChannel = threadChannel;
            return true;
        }

        guildChannel = default;
        return false;
    }
}

public readonly struct LogMessageResult
{
    public required ulong ChannelId { get; init; }
    public required ulong MessageId { get; init; }
}
