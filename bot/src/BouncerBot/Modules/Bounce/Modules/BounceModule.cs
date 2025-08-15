using BouncerBot.Attributes;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Bounce.Modules;

[SlashCommand(BounceModuleMetadata.ModuleName, BounceModuleMetadata.ModuleDescription)]
[RequireManageRoles<ApplicationCommandContext>]
[RequireGuildContext<ApplicationCommandContext>]
public class BounceModule(IBounceOrchestrator bounceOrchestrator, IBounceService bounceService) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand(BounceModuleMetadata.AddCommand.Name, BounceModuleMetadata.AddCommand.Description)]
    public async Task AddBannedHunterAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to ban")] uint hunterId,
        [SlashCommandParameter(Description = "Optional note explaining the ban")] string? note = null)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await bounceOrchestrator.AddBannedHunterAsync(hunterId, Context.Guild!.Id, note);

        await ModifyResponseWithResultAsync(result);
    }

    [SubSlashCommand(BounceModuleMetadata.RemoveCommand.Name, BounceModuleMetadata.RemoveCommand.Description)]
    public async Task RemoveBannedHunterAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to unban")] uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await bounceOrchestrator.RemoveBannedHunterAsync(hunterId, Context.Guild!.Id);

        await ModifyResponseWithResultAsync(result);
    }

    [SubSlashCommand(BounceModuleMetadata.RemoveAllCommand.Name, BounceModuleMetadata.RemoveAllCommand.Description)]
    public async Task ClearBannedHuntersAsync()
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            //Content = "Are you sure you want to clear the entire ban list for this server? This action cannot be undone.",
            Components = [
                new ComponentContainerProperties()
                    .WithComponents([
                        new TextDisplayProperties("Are you sure you want to clear the entire ban list for this server?\n\nThis action cannot be undone."),
                        new ActionRowProperties()
                            .AddButtons(new ButtonProperties($"bounce removeall:{Context.Guild!.Id}", "Confirm", ButtonStyle.Danger))
                            .AddButtons(new ButtonProperties("bounce removeall cancel", "Cancel", ButtonStyle.Secondary))
                    ])
                ],   
            Flags = MessageFlags.IsComponentsV2
        }));
    }

    [SubSlashCommand(BounceModuleMetadata.ListCommand.Name, BounceModuleMetadata.ListCommand.Description)]
    public async Task<InteractionMessageProperties> ListBannedHuntersAsync()
    {
        return new InteractionMessageProperties()
            .AddComponents(await BounceListHelper.CreateBounceListComponentsAsync(Context.Guild!.Id, 0, bounceService))
                                                 .WithFlags(MessageFlags.IsComponentsV2);
    }

    [SubSlashCommand(BounceModuleMetadata.NoteCommand.Name, BounceModuleMetadata.NoteCommand.Description)]
    public async Task UpdateBannedHunterNoteAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to update")] uint hunterId,
        [SlashCommandParameter(Description = "New note (leave blank to remove note)")] string? note = null)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await bounceOrchestrator.UpdateBannedHunterNoteAsync(hunterId, Context.Guild!.Id, note);

        await ModifyResponseWithResultAsync(result);
    }

    [SubSlashCommand(BounceModuleMetadata.CheckCommand.Name, BounceModuleMetadata.CheckCommand.Description)]
    public async Task CheckBannedHunterAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to check")] uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await bounceOrchestrator.CheckBannedHunterAsync(hunterId, Context.Guild!.Id);

        await ModifyResponseWithResultAsync(result);
    }

    private async Task ModifyResponseWithResultAsync(BounceResult result)
    {
        await ModifyResponseAsync(m =>
        {
            m.Content = result.Message;
            m.AllowedMentions = AllowedMentionsProperties.None;
        });
    }
}
