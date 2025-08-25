using BouncerBot.Db;
using BouncerBot.Services;

using Microsoft.EntityFrameworkCore;

using Scriban;

namespace BouncerBot.Modules.Achieve;

/// <summary>
/// Service for handling achievement message processing and delivery.
/// </summary>
public interface IAchievementMessageService
{
    /// <summary>
    /// Sends an achievement notification message for a user who has earned a specific achievement role.
    /// </summary>
    /// <param name="userId">The Discord user ID of the user who earned the achievement.</param>
    /// <param name="guildId">The Discord guild ID where the achievement was earned.</param>
    /// <param name="achievement">The type of achievement role that was earned.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="MessageNotConfiguredException">Thrown when no achievement message is configured for the specified guild and achievement type.</exception>
    Task SendAchievementMessageAsync(ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default);
}

public class AchievementMessageService(
    BouncerBotDbContext dbContext,
    IGuildLoggingService guildLoggingService) : IAchievementMessageService
{
    public async Task SendAchievementMessageAsync(ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        var achievementMessage = await dbContext.AchievementMessages
            .FirstOrDefaultAsync(am => am.GuildId == guildId && am.AchievementRole == achievement, cancellationToken: cancellationToken)
            ?? throw new MessageNotConfiguredException(achievement);

        var template = Template.Parse(achievementMessage.Message.Replace("{mention}", "{{mention}}"));
        var content = template.Render(new
        {
            Mention = $"<@{userId}>"
        });

        await guildLoggingService.LogAchievementAsync(guildId, achievement, content, cancellationToken);
    }
}
