namespace BouncerBot.Modules.Config.Modules;

public static class ConfigModuleMetadata
{
    public const string ModuleName = "config";
    public const string ModuleDescription = "Manage bot configuration";

    public static class LogCommand
    {
        public const string Name = "log";
        public const string Description = "Set channel where specified events go. Leave empty to clear channel.";
    }

    public static class RoleCommand
    {
        public const string Name = "role";
        public const string Description = "Set role for various bot operations";
    }

    public static class MessageCommand
    {
        public const string Name = "message";
        public const string Description = "Set message for specified achievement type. Use {mention} to mention user.";
    }

    public static class VerifyCommand
    {
        public const string Name = "verify";
        public const string Description = "Set minimum rank required for to successfully use /verify command.";
    }

    public static class ListCommand
    {
        public const string Name = "list";
        public const string Description = "List all bot configuration settings.";
    }
}
