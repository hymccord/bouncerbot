using Humanizer;
using Microsoft.EntityFrameworkCore;

using NetCord.Rest;
using NetCord;
using NetCord.Services.ComponentInteractions;
using BouncerBot.Db;

namespace BouncerBot.Modules.Variables.Modules;

public class ConfigModalInteraction(BouncerBotDbContext dbContext) : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction("variables messages modal")]
    public async Task SetMessageAsync(Role role)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var guildId = Context.Guild!.Id;
        var message = ((TextInput)Context.Components[0]).Value;

        var messages = dbContext.AchievementMessages.Find(guildId);

        if (messages is null)
        {
            messages = new Db.Models.AchievementMessage
            {
                GuildId = guildId
            };

            dbContext.AchievementMessages.Add(messages);
        }

        switch (role)
        {
            case Role.Star:
                messages.Star = message;
                break;
            case Role.Crown:
                messages.Crown = message;
                break;
            case Role.Checkmark:
                messages.Checkmark = message;
                break;
            case Role.EggMaster:
                messages.EggMaster = message;
                break;
            case Role.ArcaneMaster:
                messages.ArcaneMaster = message;
                break;
            case Role.DraconicMaster:
                messages.DraconicMaster = message;
                break;
            case Role.ForgottenMaster:
                messages.ForgottenMaster = message;
                break;
            case Role.HydroMaster:
                messages.HydroMaster = message;
                break;
            case Role.LawMaster:
                messages.LawMaster = message;
                break;
            case Role.PhysicalMaster:
                messages.PhysicalMaster = message;
                break;
            case Role.RiftMaster:
                messages.RiftMaster = message;
                break;
            case Role.ShadowMaster:
                messages.ShadowMaster = message;
                break;
            case Role.TacticalMaster:
                messages.TacticalMaster = message;
                break;
            case Role.MultiMaster:
                messages.MultiMaster = message;
                break;
            default:
                break;
        }

        await dbContext.SaveChangesAsync();

        // Since we came from a modal. There is 2nd interaction component to modify so we respond normally then delete
        var m = await FollowupAsync(new InteractionMessageProperties()
        {
            Content = $"Set {role.Humanize()} message.",
            Flags = MessageFlags.Ephemeral,
            AllowedMentions = AllowedMentionsProperties.None
        });
        await Task.Delay(3000);
        await DeleteFollowupAsync(m.Id);
    }

}
