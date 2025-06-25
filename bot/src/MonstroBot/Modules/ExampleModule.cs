using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace MonstroBot.Modules;
public class ExampleModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("pong", "Pong!")]
    public static string Pong() => "Ping!";

    [UserCommand("ID")]
    public static string Id(User user) => user.Id.ToString();

    [MessageCommand("Timestamp")]
    public static string Timestamp(RestMessage message) => message.CreatedAt.ToString();
}

[SlashCommand("verify", "123")]
//[RequireContext<ApplicationCommandContext>(RequiredContext.Guild, "These commands can only be used in a {0}.")]
public class VerifyModule : ApplicationCommandModule<ApplicationCommandContext>
{
    //[SubSlashCommand("me", "Verify you own a MouseHunt ID")]
    //public InteractionMessageProperties VerifyMe(
    //    [SlashCommandParameter(MinValue = 1)] uint mouseHuntId
    //)
    //{
    //    throw new NotImplementedException();

    //    //return new MessageProperties
    //    //{
    //    //    Content = $"You have been verified with MouseHunt ID: {mouseHuntId}",
    //    //    Flags = MessageFlags.Ephemeral
    //    //};
    //    // Here you would typically check the database to see if the user is already verified
    //    // and then add them if they are not.
    //    //await context.RespondAsync($"You have been verified with MouseHunt ID: {mouseHuntId}");
    //}

    [SubSlashCommand("remove", "Remove your MouseHunt ID verification")]
    public InteractionMessageProperties RemoveVerification()
    {
        return new InteractionMessageProperties()
            .WithFlags(MessageFlags.Ephemeral)
            .WithContent("A secret Hello!");
        // Here you would typically remove the user's verification from the database.

        return "Your MouseHunt ID verification has been removed.";
        //await context.RespondAsync("Your MouseHunt ID verification has been removed.");
    }
}


[SlashCommand("guild", "Guild command")]
[RequireContext<ApplicationCommandContext>(RequiredContext.Guild, "These commands can only be used in a server.")]
public class GuildCommandsModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("channels", "Get guild channel count")]
    public string Channels() => $"Channels: {Context.Guild!.Channels.Count}";

    [SubSlashCommand("name", "Guild name")]
    public class GuildNameModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("get", "Get guild name")]
        public string GetName() => $"Name: {Context.Guild!.Name}";

        [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
        [RequireBotPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
        [SubSlashCommand("set", "Set guild name")]
        public async Task<string> SetNameAsync(string name)
        {
            var guild = Context.Guild!;
            await guild.ModifyAsync(g => g.Name = name);
            return $"Name: {guild.Name} -> {name}";
        }
    }
}
