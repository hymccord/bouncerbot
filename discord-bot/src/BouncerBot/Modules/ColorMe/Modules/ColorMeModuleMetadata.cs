namespace BouncerBot.Modules.ColorRole.Modules;

public static class ColorMeModuleMetadata
{
    public static class ColorCommand
    {
        public const string Name = "colorme";
        public const string Description = "Choose your server display color.";
    }

    /// <summary>
    /// Select menu value for the "Remove color" option. Never collides with
    /// color values because those are numeric role IDs.
    /// </summary>
    public const string RemoveColorValue = "remove";
}
