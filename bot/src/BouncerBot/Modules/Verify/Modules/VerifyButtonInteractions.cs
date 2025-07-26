using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Verify.Modules;

public class VerifyButtonInteractions(ILogger<VerifyButtonInteractions> logger,
    VerificationOrchestrator verificationOrchestrator) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("verify user confirm")]
    public async Task VerifyUser(uint mouseHuntId, ulong discordId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var result = await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Add, new VerificationParameters
        {
            MouseHuntId = mouseHuntId,
            DiscordUserId = discordId,
            GuildId = Context.Guild!.Id,
            Phrase = string.Empty, // No phrase needed for manual verification
        });

        await ModifyResponseAsync(x =>
        {
            x.Content = result.Message;
            x.Flags = MessageFlags.Ephemeral;
            x.Components = [];
        });
    }

    [ComponentInteraction("verify user cancel")]
    public async Task CancelVerifyUser()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    [ComponentInteraction("verify remove confirm")]
    public async Task RemoveVerification(ulong discordId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var result = await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Remove, new VerificationParameters
        {
            DiscordUserId = discordId,
            GuildId = Context.Guild!.Id,
        });

        await ModifyResponseAsync(x =>
        {
            x.Content = result.Message;
            x.Flags = MessageFlags.Ephemeral;
            x.Components = [];
        });
    }

    [ComponentInteraction("verify remove cancel")]
    public async Task CancelRemoveVerification()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    private async Task DeferModifyAndDeleteResponseAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }
}
