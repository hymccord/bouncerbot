using BouncerBot.Attributes;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.WhoIs.Modules;

[SlashCommand("whois", "Look up verification information")]
[RequireManageRoles<ApplicationCommandContext>]
[RequireGuildContext<ApplicationCommandContext>]
public class WhoIsModule(IWhoIsOrchestrator whoIsOrchestrator) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("user", "Get the Hunter ID for a Discord user")]
    public async Task GetHunterIdAsync(
        [SlashCommandParameter(Description = "Discord user to look up")] User user)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await whoIsOrchestrator.GetHunterIdAsync(Context.Guild!.Id, user.Id);

        await ModifyResponseAsync(m =>
        {
            m.Embeds = [
                new EmbedProperties
                {
                    Title = "Whois User",
                    Author = new EmbedAuthorProperties
                    {
                        Name = user.Username,
                        IconUrl = user.GetAvatarUrl()?.ToString() ?? user.DefaultAvatarUrl.ToString(),
                    },
                    Description = result.Message,
                    Color = result.Success ? Colors.Green : Colors.Red,
                }
                ];
            m.AllowedMentions = AllowedMentionsProperties.None;
        });
    }

    [SubSlashCommand("hunter", "Get the Discord user for a Hunter ID")]
    public async Task GetDiscordUserAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to look up")] uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var result = await whoIsOrchestrator.GetUserIdAsync(Context.Guild!.Id, hunterId);

        await ModifyResponseAsync(m =>
        {
            m.Embeds = [
                new EmbedProperties
                {
                    Title = "Whois Hunter",
                    Description = result.Message,
                    Color = result.Success ? Colors.Green : Colors.Red,
                }
                ];
        });
    }
}
