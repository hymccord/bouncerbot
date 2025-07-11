using Humanizer;

using BouncerBot.Attributes;
using BouncerBot.Db;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Variables.Modules;

[GuildOnly<ChannelMenuInteractionContext>]
public class ConfigChannelMenuInteraction(BouncerBotDbContext dbContext) : ComponentInteractionModule<ChannelMenuInteractionContext>
{
    [ComponentInteraction("variables channels menu")]
    public async Task SetLogChannelAsync(LogChannel channel)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var guildId = Context.Guild!.Id;
        var selectedChannelId = Context.SelectedChannels.First().Id;

        var logSetting = dbContext.LogSettings.Find(guildId);

        if (logSetting is null)
        {
            logSetting = new Db.Models.LogSetting
            {
                GuildId = guildId
            };

            dbContext.LogSettings.Add(logSetting);
        }

        switch (channel)
        {
            case LogChannel.General:
                logSetting.LogId = selectedChannelId;
                break;
            case LogChannel.Achievement:
                logSetting.FlexId = selectedChannelId;
                break;
            case LogChannel.EggMaster:
                logSetting.EggMasterId = selectedChannelId;
                break;
            case LogChannel.Verification:
                logSetting.VerificationId = selectedChannelId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
        }

        await dbContext.SaveChangesAsync();
        await ModifyResponseAsync(m =>
        {
            m.Content = $"Set {channel.Humanize()} channel to <#{selectedChannelId}>.";
            m.Flags = MessageFlags.Ephemeral;
            m.Components = [];
            m.Embeds = [];
        });
        await Task.Delay(3000);
        await DeleteResponseAsync();
    }
}
