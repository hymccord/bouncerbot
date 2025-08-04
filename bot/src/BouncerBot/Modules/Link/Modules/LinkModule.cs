using BouncerBot.Attributes;
using BouncerBot.Modules.Verify;
using BouncerBot.Services;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Link.Modules;
[RequireGuildContext<ApplicationCommandContext>]
public class LinkModule(
    IRandomPhraseGenerator randomPhraseGenerator,
    IVerificationService verificationService,
    IRoleService roleService): ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("link", "Link your Discord account to your MouseHunt account.")]
    public async Task LinkAsync(
        uint hunterId)
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
                        Description = "This server does not have the Verified role configured. Please contact a moderator.",
                        Color = Colors.Red
                    }];
                m.Flags = MessageFlags.Ephemeral;
            });
        }

        var canVerifyResult = await verificationService.CanUserVerifyAsync(hunterId, Context.Guild!.Id, Context.User.Id);
        if (!canVerifyResult.CanVerify)
        {
            await ModifyResponseAsync(m =>
            {
                m.Embeds = [
                    new EmbedProperties()
                    {
                        Title = "Error",
                        Description = canVerifyResult.Message,
                        Color = Colors.Red
                    }];
                m.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        if (await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            string message = "";
            // Edge case: somehow the role was removed but the user is still linked.
            if (!await roleService.HasRoleAsync(Context.Guild.Id, Context.User.Id, Role.Verified))
            {
                message = "You are already linked, but do not have the associated role. I've added it.";
                await roleService.AddRoleAsync(Context.Guild.Id, Context.User.Id, Role.Verified);
            }
            else
            {
                message = "You are already linked!";
            }

            await ModifyResponseAsync(m =>
            {
                m.Embeds = [
                    new EmbedProperties()
                    {
                        Title = "Error",
                        Description = message,
                        Color = Colors.Yellow
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
                    Title = "Link Your Account",
                    Description = $"""
                        This process will associate your current Discord account with your MouseHunt profile.

                        Only **ONE (1)** Discord account can be associated with **ONE (1)** Hunter ID per server. You can use `/unlink` to undo this at any time, but you will need to go through this process again to re-link your account. If you wish to use a different Hunter ID than one previously linked, you will have to contact the moderators.

                        The moderators of this server will be able to access the linked user information at any time for moderation purposes.View the privacy policy with the `/privacy` command.

                        These are the details I have for you:
                        Discord: <@{Context.User.Id}> <-> MHID: {hunterId}

                        If this is correct and you agree with the above terms, place the **entire** of the following phrase on your MouseHunt profile corkboard (everything in code block):
                        ```
                        {phrase}
                        ```

                        I will read your corkboard and verify it matches.

                        Click 'Start!' to proceed, otherwise 'Cancel'.
                        """,
                    Color = Colors.Blue
                }
            ];
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
                        Color = Colors.Yellow
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
                    Color = Colors.Green
                }];
            x.Flags = MessageFlags.Ephemeral;
        });
    }
}
