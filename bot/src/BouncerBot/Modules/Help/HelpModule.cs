using BouncerBot.Attributes;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Help;


public class HelpModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("help", "Provides information about the bot and its commands.")]
    [RequireGuildContext<ApplicationCommandContext>]
    public async Task HelpAsync()
    {
        if (Context.User is GuildInteractionUser user)
        {
            Permissions perms = user.Permissions;

            List<EmbedProperties> embeds = [];

            embeds.Add(new EmbedProperties()
            {
                Title = "User Commands",
                Description = """
                - `/link <hunterId>`: Link your Discord account to your MouseHunt account.
                - `/unlink`: Unlink your Discord account from your MouseHunt account.
                - `/claim <achievement> [share]`: Claim an achievement role if you qualify for it.
                - `/help`: Provides information about the bot and its commands.
                """
            });

            if ((perms & Permissions.ManageRoles) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Moderator Commands",
                    Description = """
                    - `/verify user <hunterId> <user>`: Manually verify that a user is the owner of a MouseHunt account and link it to their Discord account.
                    - `/verify remove <user>`: Remove the verification for a user.

                    - `/achieve verify <hunterId> <achievement>`: Check if a Hunter ID qualifies for an achievement.
                    - `/achieve reset <achievement>`: Remove achievement role from all users (and grants Achiever role).
                    """,
                });
            }

            if ((perms & Permissions.ManageGuild) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Administrator Commands",
                    Description = """
                    - `/config log <type> <channel>`: Set the channel where the bot will log events. Leave blank to unset.
                    - `/config role <role> <selectedRole>`: Set the Discord role for various bot operations.
                    - `/config message <achievement> <message>`: Set the message to send when a user qualifies for a Discord Role Challenge achievement.
                    - `/config verify-rank <min_rank>`: Set the minimum MouseHunt rank required to successfully use the `/link` command. Default is "Novice".
                    - `/config list <setting>`: View the current configuration of the bot.
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
