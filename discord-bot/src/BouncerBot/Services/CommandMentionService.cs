using BouncerBot.Attributes;
using NetCord;
using NetCord.Services;
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

    IEnumerable<CommandMention> GetRegisteredCommandMentionsWithParameters();
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

        return $"</{commandName}:{mainCommand.Id}>";
    }

    public IEnumerable<CommandMention> GetRegisteredCommandMentionsWithParameters()
    {
        var commands = new List<CommandMention>();
        
        foreach (var registeredCommand in commandStorage.GetRegisteredCommands())
        {
            var commandMentions = ProcessCommandInfo(registeredCommand.Id, registeredCommand.Info, 0);
            commands.AddRange(commandMentions);
        }
        
        return commands;
    }

    /// <summary>
    /// Recursively processes command info to extract all commands and subcommands.
    /// </summary>
    private static IEnumerable<CommandMention> ProcessCommandInfo(
        ulong commandId, 
        ApplicationCommandInfo<ApplicationCommandContext> commandInfo, 
        Permissions inheritedPermissions)
    {
        return commandInfo switch
        {
            SlashCommandInfo<ApplicationCommandContext> slashCommand => 
                [CreateCommandMention(commandId, slashCommand.Name, slashCommand.Description, slashCommand.Parameters, 
                    GetEffectivePermission(slashCommand.DefaultGuildPermissions, slashCommand.Preconditions, inheritedPermissions))],
            
            SlashCommandGroupInfo<ApplicationCommandContext> group => 
                ProcessSlashCommandGroup(commandId, group, 
                    GetEffectivePermission(group.DefaultGuildPermissions, group.Preconditions, inheritedPermissions)),
            
            _ => []
        };
    }

    /// <summary>
    /// Processes a slash command group to extract all its subcommands.
    /// </summary>
    private static IEnumerable<CommandMention> ProcessSlashCommandGroup(
        ulong commandId, 
        SlashCommandGroupInfo<ApplicationCommandContext> group, 
        Permissions groupPermissions)
    {
        return group.SubCommands.Values.SelectMany(subCommand => 
            ProcessSubCommand(commandId, subCommand, groupPermissions));
    }

    /// <summary>
    /// Processes a subcommand, which can be either a SubSlashCommandInfo or SubSlashCommandGroupInfo.
    /// </summary>
    private static IEnumerable<CommandMention> ProcessSubCommand(
        ulong commandId, 
        ISubSlashCommandInfo<ApplicationCommandContext> subCommand, 
        Permissions inheritedPermissions)
    {
        return subCommand switch
        {
            SubSlashCommandInfo<ApplicationCommandContext> subSlashCommand => 
                [CreateSubCommandMention(commandId, subSlashCommand, 
                    GetEffectivePermission(null, subSlashCommand.Preconditions, inheritedPermissions))],
            
            SubSlashCommandGroupInfo<ApplicationCommandContext> subGroup => 
                ProcessSubSlashCommandGroup(commandId, subGroup, 
                    GetEffectivePermission(null, subGroup.Preconditions, inheritedPermissions)),
            
            _ => []
        };
    }

    /// <summary>
    /// Processes a sub slash command group to extract all its nested subcommands.
    /// </summary>
    private static IEnumerable<CommandMention> ProcessSubSlashCommandGroup(
        ulong commandId, 
        SubSlashCommandGroupInfo<ApplicationCommandContext> subGroup, 
        Permissions groupPermissions)
    {
        var commands = new List<CommandMention>();
        
        foreach (var nestedSubCommand in subGroup.SubCommands.Values)
        {
            var nestedCommands = ProcessSubCommand(commandId, nestedSubCommand, groupPermissions);
            commands.AddRange(nestedCommands);
        }
        
        return commands;
    }

    /// <summary>
    /// Creates a CommandMention for a top-level slash command.
    /// </summary>
    private static CommandMention CreateCommandMention(
        ulong commandId, 
        string name, 
        string description, 
        IEnumerable<SlashCommandParameter<ApplicationCommandContext>> parameters, 
        Permissions permissions)
    {
        var paramMarkdown = GetParameterMarkdown(parameters);
        var mentionMarkdown = $"</{name}:{commandId}> {paramMarkdown}: {description}".Trim();
        
        return new CommandMention(name, permissions, mentionMarkdown);
    }

    /// <summary>
    /// Creates a CommandMention for a subcommand.
    /// </summary>
    private static CommandMention CreateSubCommandMention(
        ulong commandId, 
        SubSlashCommandInfo<ApplicationCommandContext> subCommand, 
        Permissions permissions)
    {
        var commandPath = GetCommandPath(subCommand);
        var paramMarkdown = GetParameterMarkdown(subCommand.Parameters);
        var mentionMarkdown = $"</{commandPath}:{commandId}> {paramMarkdown}: {subCommand.Description}".Trim();
        
        return new CommandMention(commandPath, permissions, mentionMarkdown);
    }

    /// <summary>
    /// Extracts the full command path from a subcommand's localization path.
    /// </summary>
    private static string GetCommandPath(SubSlashCommandInfo<ApplicationCommandContext> subCommand)
    {
        return string.Join(' ', subCommand.LocalizationPath
            .Select(s => s.GetType().GetProperty("Name")?.GetValue(s)?.ToString())
            .Where(name => !string.IsNullOrEmpty(name)));
    }

    /// <summary>
    /// Generates markdown for command parameters.
    /// </summary>
    private static string GetParameterMarkdown(IEnumerable<SlashCommandParameter<ApplicationCommandContext>> parameters)
    {
        return string.Join(' ', parameters.Select(param => 
            param.IsOptional ? $"`[{param.Name}]`" : $"`<{param.Name}>`"));
    }

    /// <summary>
    /// Determines the effective permission by combining default permissions, preconditions, and inherited permissions.
    /// </summary>
    private static Permissions GetEffectivePermission(
        Permissions? defaultPermissions, 
        IEnumerable<PreconditionAttribute<ApplicationCommandContext>> preconditions, 
        Permissions inheritedPermissions)
    {
        var permission = inheritedPermissions | (defaultPermissions ?? 0);

        foreach (var precondition in preconditions.OfType<RequireUserPermissionsAttribute<ApplicationCommandContext>>())
        {
            if (precondition.ChannelPermissions.HasFlag(Permissions.ManageGuild))
            {
                permission = Permissions.ManageGuild;
            }

            if (precondition.ChannelPermissions.HasFlag(Permissions.ManageRoles))
            {
                permission = Permissions.ManageRoles;
            }
        }

        if (preconditions.OfType<RequireOwnerAttribute<ApplicationCommandContext>>().Any())
        {
            permission = Permissions.Administrator;
        }

        return permission;
    }
}

public record CommandMention(string Name, Permissions Permissions, string MentionMarkdown);
