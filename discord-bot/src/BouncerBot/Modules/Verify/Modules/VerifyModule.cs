using BouncerBot;
using BouncerBot.Attributes;
using BouncerBot.Modules.Privacy.Modules;
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
    ICommandMentionService commandMentionService,
    IRandomPhraseGenerator randomPhraseGenerator,
    IRoleService roleService,
    IVerificationOrchestrator verificationOrchestrator,
    IVerificationService verificationService,
    IBouncerBotMetrics metrics
): ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand(VerifyModuleMetadata.VerifyCommand.Name, VerifyModuleMetadata.VerifyCommand.Description)]
    public async Task VerifyAsync(
        [SlashCommandParameter(Description = "Your MouseHunt ID", MinValue = 1)]uint mousehuntID,
        [SlashCommandParameter(Description = "Use DMs to communicate with me? (If having trouble with ephemeral messages)")]bool? dm = false)
    {
        metrics.RecordCommand(VerifyModuleMetadata.VerifyCommand.Name);

        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        if (await roleService.GetRoleIdAsync(Context.Guild!.Id, Role.Verified) is null)
        {
            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Error))
                        .AddComponents(
                            new TextDisplayProperties($"""
                                This server does not have the Verified role configured. Please contact an admin!
                                """)
                        )
                    ];
                m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });
        }

        var canVerifyResult = await verificationService.CanUserVerifyAsync(mousehuntID, Context.Guild!.Id, Context.User.Id);
        if (!canVerifyResult.CanVerify)
        {
            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Error))
                        .AddComponents(
                            new TextDisplayProperties(canVerifyResult.Message)
                        )
                    ];
                m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });

            return;
        }

        if (await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            var message = "";
            // Edge case: somehow the role was removed but the user is still linked.
            if (!await roleService.HasRoleAsync(Context.User.Id, Context.Guild.Id, Role.Verified))
            {
                message = "You are already on my verified list, but do not have the associated role. I've added it!";
                await roleService.AddRoleAsync(Context.User.Id, Context.Guild.Id, Role.Verified);
            }
            else
            {
                message = "You are already verified!";
            }

            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Warning))
                        .AddComponents(
                            new TextDisplayProperties(message)
                        )
                    ];
                m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });

            return;
        }

        if (await verificationService.HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync(mousehuntID, Context.Guild.Id, Context.User.Id))
        {
            await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Add, new VerificationParameters
            {
                GuildId = Context.Guild.Id,
                DiscordUserId = Context.User.Id,
                MouseHuntId = mousehuntID,
            });

            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Success))
                        .AddComponents(
                            new TextDisplayProperties($"""
                                You previously verified with this MouseHunt ID. I've added the appropriate role!

                                -# To learn how I know this, use the {commandMentionService.GetCommandMention(PrivacyModuleMetadata.PrivacyCommand.Name)} command.
                                """)
                        )
                ];
                m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });
            return;
        }

        var phrase = $"BouncerBot Verification: {randomPhraseGenerator.Generate()}";

        IEnumerable<IMessageComponentProperties> messageComponent = [
            new ComponentContainerProperties()
            .WithAccentColor(new Color(options.Value.Colors.Warning))
            .AddComponents(
                new TextDisplayProperties("## MouseHunt Profile Verification"),
                new ComponentSeparatorProperties()
                        .WithSpacing(ComponentSeparatorSpacingSize.Large)
                        .WithDivider(true),
                new TextDisplayProperties($"""
                    :exclamation: View the privacy policy with the {commandMentionService.GetCommandMention(PrivacyModuleMetadata.PrivacyCommand.Name)} command. :exclamation:
                        
                    This process will associate your Discord account with a MouseHunt profile.

                    Only **ONE (1)** Discord account can be associated with **ONE (1)** MouseHunt ID per server. You can use {commandMentionService.GetCommandMention(VerifyModuleMetadata.UnverifyCommand.Name)} to undo this at any time. If you wish to use a different MouseHunt ID than one previously linked, you will need to contact the moderators.

                    These are the details I have for you:
                    Discord: <@{Context.User.Id}> <-> MHID: {mousehuntID}

                    If you agree with the above terms, place the **entirety** of this phrase on your MouseHunt profile corkboard (_everything_ in code block). I will read your corkboard and verify it matches.
                    ```
                    {phrase}
                    ```
                    Press 'Verify' to proceed, otherwise 'Cancel'.
                    """),
                new ComponentSeparatorProperties()
                    .WithSpacing(ComponentSeparatorSpacingSize.Large)
                    .WithDivider(true),
                new ActionRowProperties()
                    .AddComponents(new ButtonProperties($"link start:{Context.Guild!.Id}:{mousehuntID}:{phrase}", "Verify", ButtonStyle.Secondary))
                    .AddComponents(new ButtonProperties("link cancel", "Cancel", ButtonStyle.Success))
            )
        ];

        if (dm ?? false)
        {
            // User reported issues with ephemeral messages disappearing on Android, so offer to DM them instead.
            var dmChannel = await Context.User.GetDMChannelAsync();
            var sentMessage = await dmChannel.SendMessageAsync(new()
            {
                Components = messageComponent,
                Flags = MessageFlags.IsComponentsV2,
                AllowedMentions = AllowedMentionsProperties.None,
            });

            await ModifyResponseAsync(x =>
            {
                x.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Primary))
                        .AddComponents(
                            new TextDisplayProperties($"""
                                I've sent you a DM to continue the verification process. If you don't see it, check your privacy settings.
                                """)
                        )
                ];
                x.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });
        }
        else
        {
            await ModifyResponseAsync(x =>
            {
                x.Components = messageComponent;
                x.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });
        }
    }

    [SlashCommand(VerifyModuleMetadata.UnverifyCommand.Name, VerifyModuleMetadata.UnverifyCommand.Description)]
    public async Task UnverifyAsync()
    {
        metrics.RecordCommand(VerifyModuleMetadata.UnverifyCommand.Name);

        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());
        // This check is a sanity check, the precondition should ensure this is not called if the user is already verified.
        if (!await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
        {
            await ModifyResponseAsync(x =>
            {
                new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Warning))
                    .AddTextDisplay("You are already unverified!")
                    .Build(x);
                x.Flags |= MessageFlags.Ephemeral;
            });

            return;
        }

        await verificationService.RemoveVerifiedUser(Context.Guild!.Id, Context.User.Id);
        await ModifyResponseAsync(x =>
        {
            new ComponentContainerProperties()
                .WithAccentColor(new Color(options.Value.Colors.Success))
                .AddTextDisplay("""
                    You have been unverified!
                    I have removed your Discord ID and MouseHunt ID from my records.
                    """)
                .Build(x);
            x.Flags |= MessageFlags.Ephemeral;
        });
    }
}
