using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Services;
/// <summary>
/// Provides functionality to generate Discord command mentions.
/// </summary>
public interface ICommandMentionService
{
    /// <summary>
    /// Gets a formatted command mention string for the specified command name.
    /// </summary>
    /// <param name="commandName">The name of the command to get a mention for.</param>
    /// <returns>A formatted command mention string. Returns a clickable mention if the command is registered, otherwise returns a plain text format.</returns>
    string GetCommandMention(string commandName);
}

/// <summary>
/// Implementation of <see cref="ICommandMentionService"/> that generates Discord command mentions.
/// </summary>
/// <param name="commandStorage">The application command storage service used to retrieve registered commands.</param>
internal class CommandMentionService(
    IdApplicationCommandServiceStorage<ApplicationCommandContext> commandStorage
    ) : ICommandMentionService
{
    /// <summary>
    /// Gets a formatted command mention string for the specified command name.
    /// If the command is registered, returns a clickable Discord command mention.
    /// If the command is not found, returns a plain text format.
    /// </summary>
    /// <param name="commandName">The name of the command to get a mention for (case-insensitive).</param>
    /// <returns>
    /// A formatted command mention string:
    /// - If command is registered: "&lt;/commandName:commandId&gt;" (clickable mention)
    /// - If command is not found: "/commandName" (plain text)
    /// </returns>
    public string GetCommandMention(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));
        }

        var command =
            commandStorage
            .GetRegisteredCommands()
            .FirstOrDefault(rac => rac.Info.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command.Equals(default))
        {
            return $"/{commandName}";
        }

        return $"</{command.Info.Name}:{command.Id}>";
    }
}
