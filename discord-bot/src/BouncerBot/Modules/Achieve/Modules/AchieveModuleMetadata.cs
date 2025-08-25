namespace BouncerBot.Modules.Achieve.Modules;

public static class AchieveModuleMetadata
{
    public const string ModuleName = "achieve";
    public const string ModuleDescription = "Commands related to role achievements.";

    public static class VerifyCommand
    {
        public const string Name = "verify";
        public const string Description = "Check if a Hunter ID qualifies for an achievement.";
    }

    public static class ResetCommand
    {
        public const string Name = "reset";
        public const string Description = "Removes achievement role from all users (and grants Achiever)";
    }
}
