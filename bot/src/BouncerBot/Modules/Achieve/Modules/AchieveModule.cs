using Humanizer;

using BouncerBot.Attributes;
using BouncerBot.Modules.Verify;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Commands;

namespace BouncerBot.Modules.Achieve.Modules;

[GuildOnly<ApplicationCommandContext>]
[SlashCommand("achieve", "Commands related to achievements")]
public class AchieveModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("achieve", "Commands related to achievements")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Verified)]
    public async Task AchieveAsync([CommandParameter(Name = "achievement")]Role role)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"hello ${role.Humanize()}",
            Flags = MessageFlags.Ephemeral,
        }));
    }

    [SubSlashCommand("verify", "Verify another hunter")]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageRoles)]
    public async Task VerifyAsync([CommandParameter(Name = "hunter")]User user, [CommandParameter(Name = "achievement")]Role role)
    {
        
    }
}
