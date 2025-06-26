using MonstroBot.Attributes;

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


[SlashCommand("guild", "Guild command")]
[GuildOnly<SlashCommandContext>]
public class GuildCommandsModule : ApplicationCommandModule<SlashCommandContext>
{
    [SubSlashCommand("channels", "Get guild channel count")]
    public string Channels() => $"Channels: {Context.Guild!.Channels.Count}";

    [SubSlashCommand("name", "Guild name")]
    public class GuildNameModule : ApplicationCommandModule<SlashCommandContext>
    {
        [SubSlashCommand("get", "Get guild name")]
        public string GetName() => $"Name: {Context.Guild!.Name}";

        [RequireUserPermissions<SlashCommandContext>(Permissions.ManageGuild)]
        [RequireBotPermissions<SlashCommandContext>(Permissions.ManageGuild)]
        [SubSlashCommand("set", "Set guild name")]
        public async Task<string> SetNameAsync(string name)
        {
            var guild = Context.Guild!;
            await guild.ModifyAsync(g => g.Name = name);
            return $"Name: {guild.Name} -> {name}";
        }
    }
}
