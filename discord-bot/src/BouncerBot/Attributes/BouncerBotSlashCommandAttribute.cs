using NetCord;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Attributes;

/// <summary>
/// Represents a default guild slash command for bouncer bot.
/// </summary>
internal class BouncerBotSlashCommandAttribute : SlashCommandAttribute
{
    public BouncerBotSlashCommandAttribute(string name, string description)
        : base(name, description)
    {
        Contexts = [InteractionContextType.Guild];
        IntegrationTypes = [ApplicationIntegrationType.GuildInstall];
    }
}
