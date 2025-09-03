using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.WhoIs.Modules;

[SlashCommand(WhoIsModuleMetadata.ModuleName, WhoIsModuleMetadata.ModuleDescription,
        IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        Contexts = [InteractionContextType.Guild],
        DefaultGuildPermissions = Permissions.ManageRoles
)]
public class WhoIsModule(
    IOptionsSnapshot<BouncerBotOptions> options,
    IWhoIsOrchestrator whoIsOrchestrator) : ApplicationCommandModule<ApplicationCommandContext>
{
    private const int MessageTimeoutSeconds = 300;

    [SubSlashCommand(WhoIsModuleMetadata.UserCommand.Name, WhoIsModuleMetadata.UserCommand.Description)]
    public async Task GetHunterIdAsync(
        [SlashCommandParameter(Description = "Discord user to look up")] User user)
    {

        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await whoIsOrchestrator.GetHunterIdAsync(Context.Guild!.Id, user.Id);

        await ModifyWhoIsReponse("Whois User", result);
        await Task.Delay(TimeSpan.FromSeconds(MessageTimeoutSeconds));
        await TryDeleteResponseAsync();
    }

    [SubSlashCommand(WhoIsModuleMetadata.HunterCommand.Name, WhoIsModuleMetadata.HunterCommand.Description)]
    public async Task GetDiscordUserAsync(
        [SlashCommandParameter(Description = "MouseHunt ID to look up")] uint mousehuntID)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var result = await whoIsOrchestrator.GetUserIdAsync(Context.Guild!.Id, mousehuntID);

        await ModifyWhoIsReponse("Whois Hunter", result);
        await Task.Delay(TimeSpan.FromSeconds(MessageTimeoutSeconds));
        await TryDeleteResponseAsync();
    }

    private async Task ModifyWhoIsReponse(string title, WhoIsResult result)
    {
        await ModifyResponseAsync(m =>
        {
            m.Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new (result.Success ? options.Value.Colors.Success : options.Value.Colors.Error))
                    .AddComponents(
                        new TextDisplayProperties($"**{title}**"),
                        new ComponentSeparatorProperties().WithSpacing(ComponentSeparatorSpacingSize.Small).WithDivider(true),
                        new TextDisplayProperties($"""
                            {result.Message}

                            -# This message will expire in <t:{DateTimeOffset.Now.ToUnixTimeSeconds() + MessageTimeoutSeconds}:R>
                            """)
                    )
                ];
            m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            m.AllowedMentions = AllowedMentionsProperties.None;
        });
    }

    private async Task TryDeleteResponseAsync()
    {
        try
        {
            await DeleteResponseAsync();
        }
        catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Message was already deleted
        }
    }
}
