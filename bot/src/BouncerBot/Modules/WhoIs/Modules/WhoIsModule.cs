using BouncerBot.Attributes;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.WhoIs.Modules;

[SlashCommand(WhoIsModuleMetadata.ModuleName, WhoIsModuleMetadata.ModuleDescription)]
[RequireManageRoles<ApplicationCommandContext>]
[RequireGuildContext<ApplicationCommandContext>]
public class WhoIsModule(
    IOptionsSnapshot<BouncerBotOptions> options,
    IWhoIsOrchestrator whoIsOrchestrator) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand(WhoIsModuleMetadata.UserCommand.Name, WhoIsModuleMetadata.UserCommand.Description)]
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
                    Color = new (result.Success ? options.Value.Colors.Success : options.Value.Colors.Error)
                }
                ];
            m.AllowedMentions = AllowedMentionsProperties.None;
        });
    }

    [SubSlashCommand(WhoIsModuleMetadata.HunterCommand.Name, WhoIsModuleMetadata.HunterCommand.Description)]
    public async Task GetDiscordUserAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to look up")] uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await whoIsOrchestrator.GetUserIdAsync(Context.Guild!.Id, hunterId);

        await ModifyResponseAsync(m =>
        {
            m.Embeds = [
                new EmbedProperties
                {
                    Title = "Whois Hunter",
                    Description = result.Message,
                    Color = new (result.Success ? options.Value.Colors.Success : options.Value.Colors.Error)
                }
                ];
        });
    }
}
