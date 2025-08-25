using BouncerBot.Attributes;
using BouncerBot.Db;
using BouncerBot.Services;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Verification.Modules;

[SlashCommand(VerificationModuleMetadata.ModuleName, VerificationModuleMetadata.ModuleDescription)]
[RequireGuildContext<ApplicationCommandContext>]
[RequireManageRoles<ApplicationCommandContext>]
public class VerificationModule() : ApplicationCommandModule<ApplicationCommandContext>
{
#if DEBUG
    [SubSlashCommand("add", "Manually verify a MouseHunt ID and Discord user")]
    [RequireOwner<ApplicationCommandContext>]
    public async Task VerifyUserAsync(
        [SlashCommandParameter(Name = "mousehunt_id", Description = "User's MouseHunt ID")] uint hunterId,
        User user)
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Components  = [
                new ComponentContainerProperties()
                    .AddTextDisplay($"""
                    Are you sure you want to verify <@{user.Id}> as Hunter ID {hunterId}?
                    <https://p.mshnt.ca/{hunterId}>
                    """),
                new ActionRowProperties()
                        .AddButtons(new ButtonProperties($"{VerificationInteractionIds.VerifyUserConfirm}:{hunterId}:{user.Id}", "Confirm", ButtonStyle.Success))
                        .AddButtons(new ButtonProperties(VerificationInteractionIds.VerifyUserCancel, "Cancel", ButtonStyle.Danger))
            ],
            AllowedMentions = AllowedMentionsProperties.None,
            Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2
        }));
    }
#endif

    [SubSlashCommand(VerificationModuleMetadata.RemoveCommand.Name, VerificationModuleMetadata.RemoveCommand.Description)]
    [RequireGuildContext<ApplicationCommandContext>]
    [RequireManageRoles<ApplicationCommandContext>]
    public class VerifyRemoveModule(
        IOptions<BouncerBotOptions> options,
        IRoleService roleService,
        IVerificationService verificationService
    ) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand(VerificationModuleMetadata.RemoveCommand.UserCommand.Name, VerificationModuleMetadata.RemoveCommand.UserCommand.Description)]
        public async Task RemoveVerification(
            [SlashCommandParameter(Description = "A verified Discord user")] User user
        )
        {
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (!await verificationService.IsDiscordUserVerifiedAsync(Context.Guild!.Id, Context.User.Id))
            {
                await ModifyResponseAsync(x => 
                {
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Warning))
                        .AddTextDisplay("That user is not verified")
                        .Build(x);
                });
            }
            else
            {
                var verifiedRoleId = await roleService.GetRoleIdAsync(Context.Guild!.Id, Role.Verified);
                await ModifyResponseAsync(x =>
                {
                    x.Components = [
                        new ComponentContainerProperties()
                            .WithAccentColor(new Color(options.Value.Colors.Warning))
                            .AddTextDisplay("Unverify user")
                            .AddSeparator()
                            .AddTextDisplay($"""
                            Are you sure you want to remove verification for <@{user.Id}>?

                            -# Hint: This command has the same effect as removing the <@&{verifiedRoleId}> role manually from the user.
                            """),
                        new ActionRowProperties()
                            .AddButtons(new ButtonProperties($"{VerificationInteractionIds.VerifyRemoveConfirm}:{user.Id}", "Confirm", ButtonStyle.Danger))
                            .AddButtons(new ButtonProperties(VerificationInteractionIds.VerifyRemoveCancel, "Cancel", ButtonStyle.Secondary))
                        ];
                    x.Flags = MessageFlags.IsComponentsV2;
                    x.AllowedMentions = AllowedMentionsProperties.None;
                });
            }
        }

        [SubSlashCommand(VerificationModuleMetadata.RemoveCommand.HistoryCommand.Name, VerificationModuleMetadata.RemoveCommand.HistoryCommand.Description)]
        public async Task RemoveVerificationHistory(
        [SlashCommandParameter(Description = "A Discord user that has previously verified")] User user
        )
        {
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (!await verificationService.HasDiscordUserVerifiedBeforeAsync(Context.Guild!.Id, Context.User.Id))
            {
                await ModifyResponseAsync(x =>
                {
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Warning))
                        .AddTextDisplay("That user has never been verified.")
                        .Build(x);
                });
            }
            else
            {
                await ModifyResponseAsync(x =>
                {
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Warning))
                        .AddTextDisplay("MHID History Removal")
                        .AddSeparator()
                        .AddTextDisplay($"""
                            Are you sure you want to remove the MHID history for <@{user.Id}>?

                            This will allow them to verify with a different MouseHunt ID.
                            """)
                        .Build(x);
                    
                    x.AddComponents(
                        new ActionRowProperties()
                            .AddButtons(new ButtonProperties($"{VerificationInteractionIds.VerifyHistoryRemoveConfirm}:{user.Id}", "Confirm", ButtonStyle.Danger))
                            .AddButtons(new ButtonProperties(VerificationInteractionIds.VerifyHistoryRemoveCancel, "Cancel", ButtonStyle.Secondary))
                    );

                    x.AllowedMentions = AllowedMentionsProperties.None;
                });
            }
        }
    }
}
