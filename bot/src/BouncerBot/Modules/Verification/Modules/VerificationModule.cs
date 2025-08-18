using BouncerBot.Attributes;
using BouncerBot.Services;
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
            Content = $"""
                Are you sure you want to verify <@{user.Id}> as Hunter ID {hunterId}?
                <https://p.mshnt.ca/{hunterId}>
                """,

            Components =
        [
            new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"{VerificationInteractionIds.VerifyUserConfirm}:{hunterId}:{user.Id}", "Confirm", ButtonStyle.Success))
                    .AddButtons(new ButtonProperties(VerificationInteractionIds.VerifyUserCancel, "Cancel", ButtonStyle.Danger))
            ],
            AllowedMentions = AllowedMentionsProperties.None,
            Flags = MessageFlags.Ephemeral
        }));
    }
#endif

    [SubSlashCommand(VerificationModuleMetadata.RemoveCommand.Name, VerificationModuleMetadata.RemoveCommand.Description)]
    [RequireGuildContext<ApplicationCommandContext>]
    [RequireManageRoles<ApplicationCommandContext>]
    public class VerifyRemoveModule(
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
                    x.Content = "That user is not verified";
                });
            }
            else
            {
                var verifiedRoleId = await roleService.GetRoleIdAsync(Context.Guild!.Id, Role.Verified);
                await ModifyResponseAsync(x =>
                {
                    x.Content = $"""
                        Are you sure you want to remove verification for <@{user.Id}>?

                        -# Hint: This command has the same effect as removing the <@&{verifiedRoleId}> role manually from the user.
                        """;
                    
                    x.AddComponents(
                        new ActionRowProperties()
                            .AddButtons(new ButtonProperties($"{VerificationInteractionIds.VerifyRemoveConfirm}:{user.Id}", "Confirm", ButtonStyle.Danger))
                            .AddButtons(new ButtonProperties(VerificationInteractionIds.VerifyRemoveCancel, "Cancel", ButtonStyle.Secondary))
                    );

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
                    x.Content = "That user has never been verified.";
                });
            }
            else
            {
                await ModifyResponseAsync(x =>
                {
                    x.Content = $"""
                    Are you sure you want to remove historical verification for <@{user.Id}>?

                    This will allow them to verify with a different MouseHunt ID.
                    """;
                    
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
