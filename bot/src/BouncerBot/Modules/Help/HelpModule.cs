using BouncerBot.Attributes;
using BouncerBot.Services;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Help;


public class HelpModule(
    ICommandMentionService cms
    ) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("help", "Provides information about the bot and its commands.")]
    [RequireGuildContext<ApplicationCommandContext>]
    public async Task HelpAsync()
    {
        if (Context.User is GuildInteractionUser user)
        {
            var perms = user.Permissions;

            List<EmbedProperties> embeds = [];

            embeds.Add(new EmbedProperties()
            {
                Title = "User Commands",
                Description = $"""
                - {cms.GetCommandMention("link")} `<hunterId>`: Link your Discord account to your MouseHunt account.
                - {cms.GetCommandMention("unlink")}: Unlink your Discord account from your MouseHunt account.
                - {cms.GetCommandMention("claim")} `<achievement> [share]`: Claim an achievement role if you qualify for it.
                - {cms.GetCommandMention("privacy")}: View the bot's privacy policy.
                - {cms.GetCommandMention("help")}: Provides information about the bot and its commands.
                """
            });

            if ((perms & Permissions.ManageRoles) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Moderator Commands",
                    Description = $"""
                    - {cms.GetSubCommandMention("achieve verify")} `<hunterId> <achievement>`: Check if a Hunter ID qualifies for an achievement.
                    - {cms.GetSubCommandMention("achieve reset")} `<achievement>`: Remove achievement role from all users (and grants Achiever role).
                    - {cms.GetSubCommandMention("bounce add")} `<hunterId> [note]`: Ban a MouseHunt ID from using `/link`.
                    - {cms.GetSubCommandMention("bounce remove")} `<hunterId>`: Remove a MouseHunt ID from the ban list.
                    - {cms.GetSubCommandMention("bounce remove-all")}: Purge the entire ban list for this server.
                    - {cms.GetSubCommandMention("bounce list")}: View all banned MouseHunt IDs.
                    - {cms.GetSubCommandMention("bounce check")} `<hunterId>`: Check if a MouseHunt ID is banned.
                    - {cms.GetSubCommandMention("bounce note")} `<hunterId> [note]`: Update the note for a banned MouseHunt ID.
                    - {cms.GetSubCommandMention("whois user")} `<user>`: Get the Hunter ID for a Discord user.
                    - {cms.GetSubCommandMention("whois hunter")} `<hunterId>`: Get the Discord user for a Hunter ID.
                    """,
                });
            }

            if ((perms & Permissions.ManageGuild) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Administrator Commands",
                    Description = $"""
                    - {cms.GetSubCommandMention("config log")} `<type> [channel]`: Set the channel where the bot will log events. Leave blank to unset.
                    - {cms.GetSubCommandMention("config role")} `<role> <selectedRole>`: Set the Discord role for various bot operations.
                    - {cms.GetSubCommandMention("config message")} `<achievement> <message>`: Set the message to send when a user qualifies for a Discord Role Challenge achievement.
                    - {cms.GetSubCommandMention("config link")} `<min_rank>`: Set the minimum MouseHunt rank required to successfully use the `/link` command.
                    - {cms.GetSubCommandMention("config list")} `[setting]`: View the current configuration of the bot.
                    """,
                });
            }

            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            {
                Content = "Here are the commands you have access to:",
                Embeds = embeds,
                Flags = MessageFlags.Ephemeral
            }));
        }
        else
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            {
                Content = "Unable to determine your Discord permissions to see what commands you have access to.",
                Flags = MessageFlags.Ephemeral
            }));
        }
    }
}
