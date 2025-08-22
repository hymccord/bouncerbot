using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Verification.Modules;

public class VerificationButtonInteractions(
    IOptions<BouncerBotOptions> options,
    IVerificationService verificationService,
    IVerificationOrchestrator verificationOrchestrator)
    : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(VerificationInteractionIds.VerifyUserConfirm)]
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
            new ComponentContainerProperties()
                .AddTextDisplay(result.Message)
                .Build(x);
            x.Flags |= MessageFlags.Ephemeral;
        });
    }

    [ComponentInteraction(VerificationInteractionIds.VerifyUserCancel)]
    public async Task CancelVerifyUser()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    [ComponentInteraction(VerificationInteractionIds.VerifyRemoveConfirm)]
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
            new ComponentContainerProperties()
                .WithAccentColor(new Color(options.Value.Colors.Success))
                .AddTextDisplay(result.Message);
        });
    }

    [ComponentInteraction(VerificationInteractionIds.VerifyRemoveCancel)]
    public async Task CancelRemoveVerification()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    private async Task DeferModifyAndDeleteResponseAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }

    [ComponentInteraction(VerificationInteractionIds.VerifyHistoryRemoveConfirm)]
    public async Task RemoveVerificationHistory(ulong discordId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var result = await verificationService.RemoveVerificationHistoryAsync(Context.Guild!.Id, discordId);

        await ModifyResponseAsync(x =>
        {
            new ComponentContainerProperties()
                .WithAccentColor(new Color(result.WasRemoved ? options.Value.Colors.Success : options.Value.Colors.Warning))
                .AddTextDisplay(result.WasRemoved
                    ? "I've removed that user's MHID history. They can now register with any MHID!"
                    : "Sorry, I couldn't remove their history. It doesn't exist.")
                .Build(x);
        });
    }

    [ComponentInteraction(VerificationInteractionIds.VerifyHistoryRemoveCancel)]
    public async Task CancelRemoveVerificationHistory()
    {
        await DeferModifyAndDeleteResponseAsync();
    }
}
