using BouncerBot.Modules.Verify;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Link.Modules;
public class LinkButtonInteractions(
    VerificationOrchestrator verificationOrchestrator) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("link start")]
    public async Task VerifyMe(uint mouseHuntId, string phrase)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await ModifyResponseAsync(x =>
        {
            x.Content = "Please wait while I read your profile...";
            x.Flags = MessageFlags.Ephemeral;
            x.Components = [];
        });

        var verificationParameters = new VerificationParameters
        {
            MouseHuntId = mouseHuntId,
            DiscordUserId = Context.User.Id,
            GuildId = Context.Guild!.Id,
            Phrase = phrase,
        };

        var verificationResult = await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Self, verificationParameters);

        await ModifyResponseAsync(x =>
        {
            x.Content = verificationResult.Message;
            x.Flags = MessageFlags.Ephemeral;
        });

    }

    [ComponentInteraction("link cancel")]
    public async Task CancelVerification()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    private async Task DeferModifyAndDeleteResponseAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }
}
