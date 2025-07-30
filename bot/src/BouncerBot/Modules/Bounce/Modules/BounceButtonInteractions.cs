using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Bounce.Modules;
public class BounceButtonInteractions(
    IBounceOrchestrator bounceOrchestrator,
    IBounceService bounceService
    ) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("bouncelist")]
    public async Task BounceList(ulong guildId, int page)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var results = await BounceListHelper.CreateBounceListComponentsAsync(guildId, page, bounceService);

        await ModifyResponseAsync(m =>
        {
            m.AddComponents(results);
        });
    }
    [ComponentInteraction("bounce removeall")]
    public async Task ConfirmRemoveAllBannedHunters(ulong guildId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await bounceOrchestrator.RemoveAllBannedHuntersAsync(guildId);
        await ModifyResponseAsync(m =>
        {
            m.Components = [
                new TextDisplayProperties("All banned hunters have been removed from the ban list.")
                ];
            m.Flags = m.Flags;
        });
    }

    [ComponentInteraction("bounce removeall cancel")]
    public async Task CancelRemoveAllBannedHunters()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }
}
