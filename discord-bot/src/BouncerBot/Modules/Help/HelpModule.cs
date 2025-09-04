using BouncerBot.Attributes;
using BouncerBot.Services;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Help;

public class HelpModule(
    ICommandMentionService cms,
    IBouncerBotMetrics metrics
    ) : ApplicationCommandModule<ApplicationCommandContext>
{
    [BouncerBotSlashCommand("help", "Provides information about the bot and its commands.")]
    public async Task HelpAsync()
    {
        metrics.RecordCommand("help");

        if (Context.User is GuildInteractionUser user)
        {
            var cmds = cms.GetRegisteredCommandMentionsWithParameters();
            var perms = user.Permissions;

            List<IMessageComponentProperties> componentProperties = [];
            List<EmbedProperties> embeds = [];

            componentProperties.Add(
                new ComponentContainerProperties()
                    .AddTextDisplay("**User Commands**")
                    .AddSeparator()
                    .AddTextDisplay(
                        string.Join('\n', cmds.Where(c => c.Permissions == 0)
                            .OrderBy(c => c.MentionMarkdown)
                            .Select(c => $"• {c.MentionMarkdown}")
                        )
                    )
            );

            if ((perms & Permissions.ManageRoles) > 0)
            {
                componentProperties.Add(
                    new ComponentContainerProperties()
                        .AddTextDisplay("**Moderator Commands ('Manage Role' permission)**")
                        .AddSeparator()
                        .AddTextDisplay(
                            string.Join('\n', cmds.Where(c => c.Permissions.HasFlag(Permissions.ManageRoles))
                                .OrderBy(c => c.MentionMarkdown)
                                .Select(c => $"• {c.MentionMarkdown}")
                            )
                        )
                    );
            }

            if ((perms & Permissions.ManageGuild) > 0)
            {
                componentProperties.Add(
                    new ComponentContainerProperties()
                        .AddTextDisplay("**Admin Commands ('Manage Server' permission)**")
                        .AddSeparator()
                        .AddTextDisplay(
                            string.Join('\n', cmds.Where(c => c.Permissions.HasFlag(Permissions.ManageGuild))
                                .OrderBy(c => c.MentionMarkdown)
                                .Select(c => $"• {c.MentionMarkdown}")
                            )
                        )
                    );
            }

            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            {
                Components = [
                    new TextDisplayProperties("Here are the commands you have access to:"),
                    ..componentProperties
                    ],
                Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2
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
