namespace BouncerBot.Modules.Bounce.Modules;

public static class BounceModuleMetadata
{
    public const string ModuleName = "bounce";
    public const string ModuleDescription = "Manage MouseHunt ID ban list for /verify command";

    public static class AddCommand
    {
        public const string Name = "add";
        public const string Description = "Add a MouseHunt ID to the ban list";
    }

    public static class RemoveCommand
    {
        public const string Name = "remove";
        public const string Description = "Remove a MouseHunt ID from the ban list";
    }

    public static class RemoveAllCommand
    {
        public const string Name = "remove-all";
        public const string Description = "Purge the entire ban list for this server";
    }

    public static class ListCommand
    {
        public const string Name = "list";
        public const string Description = "View all banned MouseHunt IDs";
    }

    public static class NoteCommand
    {
        public const string Name = "note";
        public const string Description = "Update the note for a banned MouseHunt ID";
    }

    public static class CheckCommand
    {
        public const string Name = "check";
        public const string Description = "Check if a MouseHunt ID is banned";
    }
}
