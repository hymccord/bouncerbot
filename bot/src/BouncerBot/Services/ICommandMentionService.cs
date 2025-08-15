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

    /// <summary>
    /// Gets a formatted command mention string for the specified subcommand name.
    /// The command name should include spaces to separate the command group from subcommands.
    /// </summary>
    /// <param name="commandName">The full subcommand path (e.g., "bounce add", "config log", "whois user").</param>
    /// <returns>A formatted command mention string. Returns a clickable mention if the subcommand is registered, otherwise returns a plain text format.</returns>
    string GetSubCommandMention(string commandName);
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
            .FirstOrDefault(rac => rac.Info.Name.Equals(commandName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (command.Equals(default))
        {
            return $"/{commandName}";
        }

        return $"</{command.Info.Name}:{command.Id}>";
    }

    /// <summary>
    /// Gets a formatted command mention string for the specified subcommand name.
    /// The command name should include spaces to separate the command group from subcommands.
    /// </summary>
    /// <param name="commandName">The full subcommand path (e.g., "bounce add", "config log", "whois user").</param>
    /// <returns>
    /// A formatted command mention string:
    /// - If subcommand is registered: "&lt;/command subcommand:commandId&gt;" (clickable mention)
    /// - If subcommand is not found: "/command subcommand" (plain text)
    /// </returns>
    public string GetSubCommandMention(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));
        }

        var commandParts = commandName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (commandParts.Length == 0)
        {
            throw new ArgumentException("Command name cannot be empty after trimming.", nameof(commandName));
        }

        // Find the main command group
        var mainCommand = commandStorage
            .GetRegisteredCommands()
            .FirstOrDefault(rac => rac.Info.Name.Equals(commandParts[0], StringComparison.OrdinalIgnoreCase));

        if (mainCommand.Equals(default))
        {
            return $"/{commandName}";
        }

        // If it's just a single command, use the regular GetCommandMention logic
        if (commandParts.Length == 1)
        {
            return $"</{mainCommand.Info.Name}:{mainCommand.Id}>";
        }

        // Navigate through subcommands
        var currentInfo = mainCommand.Info;
        var commandPath = new List<string> { mainCommand.Info.Name };
        
        for (var i = 1; i < commandParts.Length; i++)
        {
            if (currentInfo is SlashCommandGroupInfo<ApplicationCommandContext> groupInfo)
            {
                var subCommandKvp = groupInfo.SubCommands
                    .FirstOrDefault(sc => sc.Key.Equals(commandParts[i], StringComparison.OrdinalIgnoreCase));
                
                if (subCommandKvp.Equals(default))
                {
                    // Subcommand not found, return plain text
                    return $"/{commandName}";
                }
                
                commandPath.Add(subCommandKvp.Key);
                
                // Check if the subcommand info is also a group for further nesting
                if (subCommandKvp.Value is SlashCommandGroupInfo<ApplicationCommandContext> nestedGroup)
                {
                    currentInfo = nestedGroup;
                }
                else
                {
                    // This is a leaf command, no more nesting possible
                    if (i < commandParts.Length - 1)
                    {
                        // Still have more parts but reached a leaf, invalid command
                        return $"/{commandName}";
                    }
                }
            }
            else
            {
                // Current command is not a group, but we still have more parts
                return $"/{commandName}";
            }
        }

        // Build the final mention with the full command path
        var fullCommandName = string.Join(" ", commandPath);
        return $"</{fullCommandName}:{mainCommand.Id}>";
    }
}
