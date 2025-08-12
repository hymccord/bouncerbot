using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Verify.Modules;

public class VerifyButtonInteractions(
    ILogger<VerifyButtonInteractions> logger,
    IVerificationService verificationService,
    IVerificationOrchestrator verificationOrchestrator)
    : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(VerifyInteractionIds.VerifyUserConfirm)]
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

    [ComponentInteraction(VerifyInteractionIds.VerifyUserCancel)]
    public async Task CancelVerifyUser()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    [ComponentInteraction(VerifyInteractionIds.VerifyRemoveConfirm)]
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

    [ComponentInteraction(VerifyInteractionIds.VerifyRemoveCancel)]
    public async Task CancelRemoveVerification()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    private async Task DeferModifyAndDeleteResponseAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }

    [ComponentInteraction(VerifyInteractionIds.VerifyHistoryRemoveConfirm)]
    public async Task RemoveVerificationHistory(ulong discordId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var result = await verificationService.RemoveVerificationHistoryAsync(Context.Guild!.Id, discordId);

        await ModifyResponseAsync(x =>
        {
            x.Content = result.WasRemoved ? "User MouseHunt ID history removed!" : "Sorry, I couldn't remove their history. It doesn't exist.";
            x.Flags = MessageFlags.Ephemeral;
            x.Components = [];
        });
    }

    [ComponentInteraction(VerifyInteractionIds.VerifyHistoryRemoveCancel)]
    public async Task CancelRemoveVerificationHistory()
    {
        await DeferModifyAndDeleteResponseAsync();
    }
}
