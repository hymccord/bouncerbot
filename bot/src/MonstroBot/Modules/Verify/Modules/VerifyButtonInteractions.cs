using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace MonstroBot.Modules.Verify.Modules;

public class VerifyButtonInteractions(ILogger<VerifyButtonInteractions> logger,
    VerificationService verificationService
) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("verify me start")]
    public async Task VerifyMe(uint mouseHuntId, string phrase)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await ModifyResponseAsync(x =>
        {
            x.Content = "Please wait while I read your profile...";
            x.Flags = MessageFlags.Ephemeral;
            x.Components = [];
        });

        logger.LogDebug("Verifying user {MouseHuntId} with phrase: {Phrase}", mouseHuntId, phrase);

        if (!await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            await verificationService.AddVerifiedUserAsync(mouseHuntId, Context.Guild!.Id, Context.User.Id);
        }
        else
        {
            logger.LogInformation("User {UserId} is already verified in guild {GuildId}", Context.User.Id, Context.Guild!.Id);
        }

        await ModifyResponseAsync(x =>
        {
            x.Content = $"""
            Success! You may now remove the phrase from your corkboard.
            """;
            x.Flags = MessageFlags.Ephemeral;
        });
    }

    [ComponentInteraction("verify me cancel")]
    public async Task CancelVerification()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await DeleteResponseAsync();
    }

    [ComponentInteraction("verify remove confirm")]
    public async Task RemoveVerification(ulong discordId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await verificationService.RemoveVerifiedUser(Context.Guild!.Id, discordId);

        await ModifyResponseAsync(x =>
        {
            x.Content = $"Removed verification for <@{discordId}>.";
            x.Flags = MessageFlags.Ephemeral;
            x.Components = [];
        });
    }

    [ComponentInteraction("verify remove cancel")]
    public async Task CancelRemoveVerification()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await DeleteResponseAsync();
    }
}
