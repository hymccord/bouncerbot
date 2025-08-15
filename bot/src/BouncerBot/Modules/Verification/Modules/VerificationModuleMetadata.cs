namespace BouncerBot.Modules.Verification.Modules;

internal class VerificationModuleMetadata
{
    public const string ModuleName = "verification";
    public const string ModuleDescription = "Manage MouseHunt ID verification.";

    public static class RemoveCommand
    {
        public const string Name = "remove";
        public const string Description = "Manage verification removal.";

        public static class UserCommand
        {
            public const string Name = "user";
            public const string Description = "Remove a user from the verification list.";
        }

        public static class HistoryCommand
        {
            public const string Name = "history";
            public const string Description = "Remove a user's verification history.";
        }
    }
}
