using BouncerBot.Attributes;
using BouncerBot.Modules.Verify;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Link.Modules;
[RequireGuildContext<ApplicationCommandContext>]
public class LinkModule(
    IRandomPhraseGenerator randomPhraseGenerator,
    VerificationService verificationService,
    VerificationOrchestrator verificationOrchestrator): ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("link", "Link your Discord account to your MouseHunt account.")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Unverified)]
    public async Task LinkAsync(
        uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var canVerifyResult = await verificationService.CanUserVerifyAsync(hunterId, Context.Guild!.Id, Context.User.Id);
        if (!canVerifyResult.CanVerify)
        {
            await ModifyResponseAsync(m =>
            {
                m.Content = canVerifyResult.Message;
                m.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        // This check is a sanity check, the precondition should ensure this is not called if the user is already verified.
        if (await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            await ModifyResponseAsync(x =>
            {
                x.Content = "You are already linked!";
                x.Flags = MessageFlags.Ephemeral;
            });

            return;
        }
        var phrase = randomPhraseGenerator.Generate();
        await ModifyResponseAsync(x =>
        {
            x.Content = $"""
            This process will associate your current Discord account with your MouseHunt profile.

            Only **ONE (1)** Discord account can be associated with **ONE (1)** MouseHunt account.

            These are the details I have for you:
            Discord: <@{Context.User.Id}> <-> MHID: {hunterId}

            If this is correct, place the **entire** of the following phrase on your MouseHunt profile corkboard (everything in code block):
            ```
            {phrase}
            ```

            I will read your corkboard and verify it matches.

            Click 'Start!' to proceed, otherwise 'Cancel'.
            """;
            x.AddComponents(
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"link start:{hunterId}:{phrase}", "Start!", ButtonStyle.Success))
                    .AddButtons(new ButtonProperties("link cancel", "Cancel", ButtonStyle.Danger))
                );
            x.AllowedMentions = AllowedMentionsProperties.None;
        });
    }

    [SlashCommand("unlink", "Unlink your Discord account from your MouseHunt account.")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Verified)]
    public async Task UnlinkAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
        // This check is a sanity check, the precondition should ensure this is not called if the user is already verified.
        if (await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            await ModifyResponseAsync(x =>
            {
                x.Content = "Your Discord account is not linked to a Hunter ID!";
                x.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        await verificationService.RemoveVerifiedUser(Context.Guild!.Id, Context.User.Id);
        await ModifyResponseAsync(x =>
        {
            x.Content = "Your Discord account has been unlinked from your MouseHunt account.";
            x.Flags = MessageFlags.Ephemeral;
        });
    }
}
