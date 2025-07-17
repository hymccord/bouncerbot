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
                - `/verify me`: Verify that you are the owner of a MouseHunt account and link it to your Discord account.
                - `/achieve [achievement]`: Check if you qualify a specific Discord Challenge role.
                - `/help`: Provides information about the bot and its commands.
                """
            });

            if ((perms & Permissions.ManageRoles) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Moderator Commands",
                    Description = """
                    - `/verify [user] [mhid]`: Manually verify that a user is the owner of a MouseHunt account and link it to their Discord account.
                    - `/verify remove [user]`: Remove the verification for a user.
                    - `/verify achievement [achievement] [mhid|user]`: Check if a user qualifies for a specific Discord Challenge role.

                    - `/watchlist add [mhid]`: Block a MouseHunt ID from using `/verify me`.
                    - `/watchlist remove [mhid]`: Remove a MouseHunt ID from the watchlist.
                    - `/watchlist list`: List all MouseHunt IDs on the watchlist.

                    - `/config view`: View the current configuration of the bot.
                    """,
                });
            }

            if ((perms & Permissions.Administrator) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Administrator Commands",
                    Description = """
                    - `/config log [type] [channel]`: Set the channel where the bot will log events. Leave blank to unset.
                    - `/config role [achievement] [role]`: Set the Discord role for a MouseHunt "Discord Role Challenge" achievement.
                    - `/config message [achievement] [message]`: Set the message to send when a user qualifies for a Discord Role Challenge achievement.
                    - `/config verify [rank]`: Set the minimum MouseHunt rank required to successfully use the `/verify me` command. Default is "Novice".
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
