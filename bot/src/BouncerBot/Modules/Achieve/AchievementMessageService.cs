using BouncerBot.Db;
using BouncerBot.Services;

using Scriban;

namespace BouncerBot.Modules.Achieve;

public class AchievementMessageService(BouncerBotDbContext dbContext, IGuildLoggingService guildLoggingService)
{
    public async Task SendAchievementMessageAsync(ulong userId, ulong guildId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        var achievementMessage = await dbContext.AchievementMessages.FindAsync(guildId) switch
        {
            { Star: var message } when achievement == AchievementRole.Star => message,
            { Crown: var message } when achievement == AchievementRole.Crown => message,
            { Checkmark: var message } when achievement == AchievementRole.Checkmark => message,
            { EggMaster: var message } when achievement == AchievementRole.EggMaster => message,
            { ArcaneMaster: var message } when achievement == AchievementRole.ArcaneMaster => message,
            { DraconicMaster: var message } when achievement == AchievementRole.DraconicMaster => message,
            { ForgottenMaster: var message } when achievement == AchievementRole.ForgottenMaster => message,
            { HydroMaster: var message } when achievement == AchievementRole.HydroMaster => message,
            { LawMaster: var message } when achievement == AchievementRole.LawMaster => message,
            { PhysicalMaster: var message } when achievement == AchievementRole.PhysicalMaster => message,
            { RiftMaster: var message } when achievement == AchievementRole.RiftMaster => message,
            { ShadowMaster: var message } when achievement == AchievementRole.ShadowMaster => message,
            { TacticalMaster: var message } when achievement == AchievementRole.TacticalMaster => message,
            { MultiMaster: var message } when achievement == AchievementRole.MultiMaster => message,
            _ => null
        } ?? throw new InvalidOperationException($"The message for this achievement has not been configured yet. An admin needs to use `/config message`.");

        var template = Template.Parse(achievementMessage.Replace("{mention}", "{{mention}}"));
        var result = template.Render(new
        {
            Mention = $"<@{userId}>"
        });

        await guildLoggingService.LogAsync(guildId, LogType.Achievement, new()
        {
            Content = result
        });
    }
}
