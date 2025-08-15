using BouncerBot;
using BouncerBot.Attributes;
using BouncerBot.Modules.Verification;
using BouncerBot.Services;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Verify.Modules;
[RequireGuildContext<ApplicationCommandContext>]
public class VerifyModule(
    IOptionsSnapshot<BouncerBotOptions> options,
    IRandomPhraseGenerator randomPhraseGenerator,
    IVerificationService verificationService,
    IRoleService roleService,
    ICommandMentionService commandMentionService): ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("verify", "Verify that you own a MouseHunt account.")]
    public async Task LinkAsync(
        [SlashCommandParameter(Description = "Your MouseHunt ID", MinValue = 1)]
        uint mousehuntID)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        if (await roleService.GetRoleIdAsync(Context.Guild!.Id, Role.Verified) is null)
        {
            await ModifyResponseAsync(m =>
            {
                m.Embeds = [
                    new EmbedProperties()
                    {
                        Title = "Error",
                        Description = "This server does not have the Verified role configured. Please contact an admin!",
                        Color = new(options.Value.Colors.Error)
                    }];
                m.Flags = MessageFlags.Ephemeral;
            });
        }

        var canVerifyResult = await verificationService.CanUserVerifyAsync(mousehuntID, Context.Guild!.Id, Context.User.Id);
        if (!canVerifyResult.CanVerify)
        {
            await ModifyResponseAsync(m =>
            {
                m.Embeds = [
                    new EmbedProperties()
                    {
                        Title = "Error",
                        Description = canVerifyResult.Message,
                        Color = new(options.Value.Colors.Error)
                    }];
                m.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        if (await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            var message = "";
            // Edge case: somehow the role was removed but the user is still linked.
            if (!await roleService.HasRoleAsync(Context.Guild.Id, Context.User.Id, Role.Verified))
            {
                message = "You are already verified, but do not have the associated role. I've added it.";
                await roleService.AddRoleAsync(Context.Guild.Id, Context.User.Id, Role.Verified);
            }
            else
            {
                message = "You are already verified!";
            }

            await ModifyResponseAsync(m =>
            {
                m.Embeds = [
                    new EmbedProperties()
                    {
                        Title = "Error",
                        Description = message,
                        Color = new(options.Value.Colors.Warning)
                    }];
                m.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        if (await verificationService.HasDiscordUserVerifiedBeforeAsync(mousehuntID, Context.Guild.Id, Context.User.Id))
        {
            await ModifyResponseAsync(m =>
            {
                m.Embeds = [
                    new EmbedProperties()
                    {
                        Title = "Success",
                        Description = $"""
                        You previously verified with this MouseHunt ID. I've added the appropriate role!

                        -# To learn how I know this, use the {commandMentionService.GetCommandMention("privacy")} command.
                        """,
                        Color = new (options.Value.Colors.Success)
                    }];
                m.Flags = MessageFlags.Ephemeral;
            });
            return;
        }

        var phrase = $"BouncerBot Discord Link: {randomPhraseGenerator.Generate()}";
        await ModifyResponseAsync(x =>
        {
            x.Embeds = [
                new EmbedProperties()
                {
                    Title = "MouseHunt Account Verification",
                    Description = $"""
                        This process will associate your current Discord account with your MouseHunt profile.

                        Only **ONE (1)** Discord account can be associated with **ONE (1)** Hunter ID per server. You can use {commandMentionService.GetCommandMention("verify")} to undo this at any time, but you will need to go through this process again to re-link your account. If you wish to use a different Hunter ID than one previously linked, you will have to contact the moderators.

                        The moderators of this server will be able to access the linked user information at any time for moderation purposes.
                        View the privacy policy with the {commandMentionService.GetCommandMention("privacy")} command.

                        These are the details I have for you:
                        Discord: <@{Context.User.Id}> <-> MHID: {mousehuntID}

                        If this is correct and you agree with the above terms, place the **entire** of the following phrase on your MouseHunt profile corkboard (everything in code block):
                        ```
                        {phrase}
                        ```

                        I will read your corkboard and verify it matches.

                        Click 'Start!' to proceed, otherwise 'Cancel'.
                        """,
                    Color = new(options.Value.Colors.Primary)
                }
            ];
            x.AddComponents(
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"link start:{mousehuntID}:{phrase}", "Start!", ButtonStyle.Success))
                    .AddButtons(new ButtonProperties("link cancel", "Cancel", ButtonStyle.Danger))
                );
            x.AllowedMentions = AllowedMentionsProperties.None;
        });
    }

    [SlashCommand("unverify", "Unlink your Discord account from your MouseHunt account.")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Verified)]
    public async Task UnlinkAsync()
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());
        // This check is a sanity check, the precondition should ensure this is not called if the user is already verified.
        if (await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            await ModifyResponseAsync(x =>
            {
                x.Embeds = [
                    new EmbedProperties()
                    {
                        Title = "Error",
                        Description = "You are already unlinked!",
                        Color = new(options.Value.Colors.Warning)
                    }];
                x.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        await verificationService.RemoveVerifiedUser(Context.Guild!.Id, Context.User.Id);
        await ModifyResponseAsync(x =>
        {
            x.Embeds = [
                new EmbedProperties()
                {
                    Title = "Unlinked",
                    Description = "Your Discord account has been unlinked from your MouseHunt account.",
                    Color = new (options.Value.Colors.Success)
                }];
            x.Flags = MessageFlags.Ephemeral;
        });
    }
}
