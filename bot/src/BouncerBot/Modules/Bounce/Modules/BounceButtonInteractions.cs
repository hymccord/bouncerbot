using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Bounce.Modules;
public class BounceButtonInteractions(IBounceService bounceService) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("bouncelist")]
    public async Task<InteractionCallbackProperties> BounceList(ulong guildId, int page)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var results = await BounceListHelper.CreateBounceListComponentsAsync(guildId, page, bounceService);

        return InteractionCallback.ModifyMessage(m =>
        {
            m.AddComponents(results);
        });
    }
}
