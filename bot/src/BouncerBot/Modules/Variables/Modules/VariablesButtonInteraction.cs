using Humanizer;

using BouncerBot.Attributes;
using BouncerBot.Db;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Variables.Modules;

[GuildOnly<ButtonInteractionContext>]
public class VariablesButtonInteraction(BouncerBotDbContext dbContext) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("variables channels")]
    public async Task CreateChannelMenuAsync(LogChannel channel)
    {
        //await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var defaultValue = dbContext.LogSettings.Find(Context.Guild!.Id) switch
        {
            { LogId: var id } when channel == LogChannel.General => id,
            { FlexId: var id } when channel == LogChannel.Achievement => id,
            { EggMasterId: var id } when channel == LogChannel.EggMaster => id,
            { VerificationId: var id } when channel == LogChannel.Verification => id,
            _ => 0UL
        };

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        //await ModifyResponseAsync(m =>
        {
            Embeds = [
                new EmbedProperties() {
                    Title = $"Editing the {channel.Humanize()} channel setting",
                }],
            Components = [
                new ChannelMenuProperties($"variables channels menu:{(int)channel}")
                {
                    ChannelTypes = [ ChannelType.PublicGuildThread, ChannelType.TextGuildChannel ],
                    DefaultValues = [defaultValue ?? 0]
                }
            ],
            Flags = MessageFlags.Ephemeral
        }));
    }

    [ComponentInteraction("variables roles")]
    public async Task CreateRoleMenuAsync(Role roleSetting)
    {
        //await RespondAsync(InteractionCallback.DeferredModifyMessage);
        var defaultValue = dbContext.RoleSettings.Find(Context.Guild!.Id) switch
        {
            { StarId: var id } when roleSetting == Role.Star => id,
            { CrownId: var id } when roleSetting == Role.Crown => id,
            { CheckmarkId: var id } when roleSetting == Role.Checkmark => id,
            { EggMasterId: var id } when roleSetting == Role.EggMaster => id,
            { ArcaneMasterId: var id } when roleSetting == Role.ArcaneMaster => id,
            { DraconicMasterId: var id } when roleSetting == Role.DraconicMaster => id,
            { ForgottenMasterId: var id } when roleSetting == Role.ForgottenMaster => id,
            { HydroMasterId: var id } when roleSetting == Role.HydroMaster => id,
            { LawMasterId: var id } when roleSetting == Role.LawMaster => id,
            { PhysicalMasterId: var id } when roleSetting == Role.PhysicalMaster => id,
            { RiftMasterId: var id } when roleSetting == Role.RiftMaster => id,
            { ShadowMasterId: var id } when roleSetting == Role.ShadowMaster => id,
            { TacticalMasterId: var id } when roleSetting == Role.TacticalMaster => id,
            _ => 0UL
        };
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        //await ModifyResponseAsync(m =>
        {
            Embeds = [
                new EmbedProperties() {
                    Title = $"Editing the {roleSetting.Humanize()} role setting",
                }],
            Components = [
                new RoleMenuProperties($"variables roles menu:{(int)roleSetting}")
                {
                    DefaultValues = [defaultValue ?? 0]
                }
            ],
            Flags = MessageFlags.Ephemeral
        }));
    }

    [ComponentInteraction("variables messages")]
    public async Task CreateMessageMenuAsync(Role role)
    {
        //await RespondAsync(InteractionCallback.DeferredModifyMessage);
        var defaultValue = dbContext.AchievementMessages.Find(Context.Guild!.Id) switch
        {
            { Star: var message } when role == Role.Star => message,
            { Crown: var message } when role == Role.Crown => message,
            { Checkmark: var message } when role == Role.Checkmark => message,
            { EggMaster: var message } when role == Role.EggMaster => message,
            { ArcaneMaster: var message } when role == Role.ArcaneMaster => message,
            { DraconicMaster: var message } when role == Role.DraconicMaster => message,
            { ForgottenMaster: var message } when role == Role.ForgottenMaster => message,
            { HydroMaster: var message } when role == Role.HydroMaster => message,
            { LawMaster: var message } when role == Role.LawMaster => message,
            { PhysicalMaster: var message } when role == Role.PhysicalMaster => message,
            { RiftMaster: var message } when role == Role.RiftMaster => message,
            { ShadowMaster: var message } when role == Role.ShadowMaster => message,
            { TacticalMaster: var message } when role == Role.TacticalMaster => message,
            _ => string.Empty
        };

        await RespondAsync(InteractionCallback.Modal(new ModalProperties($"variables messages modal:{(int)role}", "", [
            new TextInputProperties($"message", TextInputStyle.Paragraph, $"{role.Humanize()} message") {
                Value = defaultValue,
                Placeholder = "Use {mention} to mention the user."
            }
        ])));
    }
}
