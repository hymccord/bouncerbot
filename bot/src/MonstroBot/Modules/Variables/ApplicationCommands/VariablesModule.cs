using MonstroBot.Attributes;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace MonstroBot.Modules.Variables.ApplicationCommands;

[SlashCommand("variables", "Manage variables for the bot.")]
[GuildOnly<ApplicationCommandContext>]
public class VariablesModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("channels", "Channel settings")]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.Administrator)]
    public async Task SetChannelVariables()
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Components = [
                new ActionRowProperties().WithButtons([new ButtonProperties("button-123", "Label 123", ButtonStyle.Primary)]),
                //new ChannelMenuProperties("channel-select-1")
                //{
                //    Placeholder = "Select a channel",
                //    ChannelTypes = [ChannelType.PublicGuildThread, ChannelType.TextGuildChannel]
                //},
                new ChannelMenuProperties("channel-select-2")
                {
                    Placeholder = "Select a channel",
                    DefaultValues = [1387188355854372984],
                    ChannelTypes = [ChannelType.PublicGuildThread, ChannelType.TextGuildChannel]
                },
                new ChannelMenuProperties("channel-select-3")
                {
                    Placeholder = "Select a channel",
                    ChannelTypes = [ChannelType.PublicGuildThread, ChannelType.TextGuildChannel]
                },
                new ChannelMenuProperties("channel-select-4")
                {
                    Placeholder = "Select a channel",
                    ChannelTypes = [ChannelType.PublicGuildThread, ChannelType.TextGuildChannel]
                },
                new ChannelMenuProperties("channel-select-5")
                {
                    Placeholder = "Select a channel",
                    ChannelTypes = [ChannelType.PublicGuildThread, ChannelType.TextGuildChannel]
                }
            ],
            Flags = MessageFlags.Ephemeral
        }));

        MessageProperties msg = "123";
        await FollowupAsync(new()
        {
            Components = [
                new ActionRowProperties().WithButtons([new ButtonProperties("button-123", "Label 123", ButtonStyle.Primary)]),

            ],
            Flags = MessageFlags.Ephemeral
        });
    }

    [SubSlashCommand("users", "Role settings")]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.Administrator)]
    public async Task SetUserVariables()
    {
    }
}
