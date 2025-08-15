namespace BouncerBot.Modules.WhoIs.Modules;

public static class WhoIsModuleMetadata
{
    public const string ModuleName = "whois";
    public const string ModuleDescription = "Look up verification information";

    public static class UserCommand
    {
        public const string Name = "user";
        public const string Description = "Get the Hunter ID for a Discord user";
    }

    public static class HunterCommand
    {
        public const string Name = "hunter";
        public const string Description = "Get the Discord user for a Hunter ID";
    }
}
