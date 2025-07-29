using BouncerBot.Attributes;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Bounce.Modules;

[SlashCommand("bounce", "Manage MouseHunt ID ban list for /link command")]
[RequireManageRoles<ApplicationCommandContext>]
[RequireGuildContext<ApplicationCommandContext>]
public class BounceModule(IBounceOrchestrator bounceOrchestrator, IBounceService bounceService) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("add", "Add a MouseHunt ID to the ban list")]
    public async Task AddBannedHunterAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to ban")] uint hunterId,
        [SlashCommandParameter(Description = "Optional note explaining the ban")] string? note = null)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await bounceOrchestrator.AddBannedHunterAsync(hunterId, Context.Guild!.Id, note);

        await ModifyResponseWithResultAsync(result);
    }

    [SubSlashCommand("remove", "Remove a MouseHunt ID from the ban list")]
    public async Task RemoveBannedHunterAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to unban")] uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var result = await bounceOrchestrator.RemoveBannedHunterAsync(hunterId, Context.Guild!.Id);

        await ModifyResponseWithResultAsync(result);

    }

    [SubSlashCommand("list", "View all banned MouseHunt IDs")]
    public async Task<InteractionMessageProperties> ListBannedHuntersAsync()
    {
        return new InteractionMessageProperties()
            .AddComponents(await BounceListHelper.CreateBounceListComponentsAsync(Context.Guild!.Id, 0, bounceService))
                                                 .WithFlags(MessageFlags.IsComponentsV2);
    }

    [SubSlashCommand("note", "Update the note for a banned MouseHunt ID")]
    public async Task UpdateBannedHunterNoteAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to update")] uint hunterId,
        [SlashCommandParameter(Description = "New note (leave blank to remove note)")] string? note = null)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var result = await bounceOrchestrator.UpdateBannedHunterNoteAsync(hunterId, Context.Guild!.Id, note);

        await ModifyResponseWithResultAsync(result);

    }

    [SubSlashCommand("check", "Check if a MouseHunt ID is banned")]
    public async Task CheckBannedHunterAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to check")] uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var result = await bounceOrchestrator.CheckBannedHunterAsync(hunterId, Context.Guild!.Id);

        await ModifyResponseWithResultAsync(result);
    }

    async Task ModifyResponseWithResultAsync(BounceResult result)
    {
        await ModifyResponseAsync(m =>
        {
            m.Content = result.Message;
            m.AllowedMentions = AllowedMentionsProperties.None;
        });
    }
}
