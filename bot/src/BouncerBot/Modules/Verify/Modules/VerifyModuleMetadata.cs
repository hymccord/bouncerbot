namespace BouncerBot.Modules.Verify.Modules;

public static class VerifyModuleMetadata
{
    public static class VerifyCommand
    {
        public const string Name = "verify";
        public const string Description = "Verify that you own a MouseHunt account.";
    }

    public static class UnverifyCommand
    {
        public const string Name = "unverify";
        public const string Description = "Unlink your Discord account from your MouseHunt account.";
    }
}
