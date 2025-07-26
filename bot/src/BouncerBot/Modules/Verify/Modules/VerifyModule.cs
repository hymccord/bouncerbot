using BouncerBot.Attributes;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Verify.Modules;

[SlashCommand("verify", "Manage MouseHunt ID verification")]
[RequireGuildContext<ApplicationCommandContext>]
public class VerifyModule(
    IVerificationService verificationService) : ApplicationCommandModule<ApplicationCommandContext>
{
    // Only owner for now: testing
    [SubSlashCommand("user", "Manually verify a MouseHunt ID and Discord user")]
    [RequireOwner<ApplicationCommandContext>]
    //[RequireManageRoles<ApplicationCommandContext>]
    public async Task VerifyUserAsync(
        [SlashCommandParameter(Name = "mousehunt_id", Description = "User's MouseHunt ID")] uint hunterId,
        User user)
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Content = $"""
                Are you sure you want to link <@{user.Id}> as Hunter ID {hunterId}?
                <https://p.mshnt.ca/{hunterId}>
                """,

            Components =
            [
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"verify user confirm:{hunterId}:{user.Id}", "Confirm", ButtonStyle.Success))
                    .AddButtons(new ButtonProperties("verify user cancel", "Cancel", ButtonStyle.Danger))
            ],
            AllowedMentions = AllowedMentionsProperties.None,
            Flags = MessageFlags.Ephemeral
        }));
    }

    [SubSlashCommand("remove", "Remove a MouseHunt ID verification")]
    [RequireManageRoles<ApplicationCommandContext>]
    public async Task RemoveVerification(
        [SlashCommandParameter(Description = "A verified Discord user")] User user
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
