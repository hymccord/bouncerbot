using Microsoft.Extensions.Logging;

using BouncerBot.Attributes;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Verify.Modules;

[SlashCommand("verify", "Manage MouseHunt ID verification")]
[GuildOnly<ApplicationCommandContext>]
public class VerifyModule(ILogger<VerifyModule> logger,
    VerificationService verificationService,
    IVerificationPhraseGenerator verificationPhraseGenerator) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("me", "Verify you own a MouseHunt ID")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Unverified)]
    public async Task VerifyMe(
        [SlashCommandParameter(Description = "MouseHunt Profile ID", Name = "id", MinValue = 1)] uint mouseHuntId
    )
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        // This check is a sanity check, the precondition should ensure this is not called if the user is already verified.
        if (await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            await ModifyResponseAsync(x =>
            {
                x.Content = "You are already verified!";
                x.Flags = MessageFlags.Ephemeral;
            });

            return;
        }
        var phrase = verificationPhraseGenerator.GeneratePhrase();
        await ModifyResponseAsync(x =>
        {
            x.Content = $"""
            This process will associate your current Discord account with your MouseHunt profile.

            Only **ONE (1)** Discord account can be associated with **ONE (1)** MouseHunt account.

            Be sure these are the accounts you want to associate.
            Discord: <@{Context.User.Id}> <-> MHID: {mouseHuntId}

            If you are sure this is correct, place the **entire** of the following phrase on your MouseHunt profile corkboard (everything in code block):
            ```
            {phrase}
            ```

            I will read your corkboard and verify it matches.

            Click 'Start!' to proceed, otherwise 'Cancel'.
            """;
            x.AddComponents(
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"verify me start:{mouseHuntId}:{phrase}", "Start!", ButtonStyle.Success))
                    .AddButtons(new ButtonProperties("verify me cancel", "Cancel", ButtonStyle.Danger))
                );
            x.AllowedMentions = AllowedMentionsProperties.None;
        });
    }

    [SubSlashCommand("user", "Manually verify a MouseHunt ID and Discord user")]
    [ManageRolesOnly<ApplicationCommandContext>]
    public async Task VerifyUserAsync(
        [SlashCommandParameter(Name = "mousehunt_id", Description = "User's MouseHunt ID")] uint mhid,
        User user)
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Content = $"""
                Are you sure you want to verify <@{user.Id}> as hunter {mhid}?
                <https://p.mshnt.ca/{mhid}>
                """,
            
            Components =
            [
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"verify user confirm:{mhid}:{user.Id}", "Confirm", ButtonStyle.Success))
                    .AddButtons(new ButtonProperties("verify user cancel", "Cancel", ButtonStyle.Danger))
            ],
            AllowedMentions = AllowedMentionsProperties.None,
            Flags = MessageFlags.Ephemeral
        }));
    }

    [SubSlashCommand("remove", "Remove a MouseHunt ID verification")]
    [ManageRolesOnly<ApplicationCommandContext>]
    public async Task RemoveVerification(
        [SlashCommandParameter(Description = "A verified Discord user")]User user
        )
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        if (!await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            await ModifyResponseAsync(x =>
            {
                x.Content = "That user is not verified";
            });
        }
        else
        {
            await ModifyResponseAsync(x =>
            {
                x.Content = $"Are you sure you want to remove verification for <@{user.Id}>?";
                x.AddComponents(
                    new ActionRowProperties()
                        .AddButtons(new ButtonProperties($"verify remove confirm:{user.Id}", "Confirm", ButtonStyle.Danger))
                        .AddButtons(new ButtonProperties("verify remove cancel", "Cancel", ButtonStyle.Secondary))
                );
            });
        }
    }
}
