using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Verify.Modules;

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
        await DeferModifyAndDeleteResponseAsync();
    }

    [ComponentInteraction("verify user confirm")]
    public async Task VerifyUser(uint mouseHuntId, ulong discordId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        if (!await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, discordId))
        {
            await verificationService.AddVerifiedUserAsync(mouseHuntId, Context.Guild!.Id, discordId);
            await ModifyResponseAsync(x =>
            {
                x.Content = $"Verified <@{discordId}> to be hunter {mouseHuntId}.";
                x.Flags = MessageFlags.Ephemeral;
                x.Components = [];
            });
        }
        else
        {
            logger.LogInformation("User {UserId} is already verified in guild {GuildId}", discordId, Context.Guild!.Id);
            await ModifyResponseAsync(x =>
            {
                x.Content = $"<@{discordId}> is already verified.";
                x.Flags = MessageFlags.Ephemeral;
                x.Components = [];
            });
        }
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
        await DeferModifyAndDeleteResponseAsync();
    }

    private async Task DeferModifyAndDeleteResponseAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }
}
