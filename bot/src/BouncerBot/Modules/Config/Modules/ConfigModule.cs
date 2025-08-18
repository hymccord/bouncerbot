using BouncerBot;
using BouncerBot.Attributes;

using Humanizer;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Config.Modules;

[SlashCommand(ConfigModuleMetadata.ModuleName, ConfigModuleMetadata.ModuleDescription)]
[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
[RequireGuildContext<ApplicationCommandContext>]
public partial class ConfigModule(
    IOptionsSnapshot<BouncerBotOptions> options,
    IConfigService configService)
    : ApplicationCommandModule<ApplicationCommandContext>
{

    [SubSlashCommand(ConfigModuleMetadata.LogCommand.Name, ConfigModuleMetadata.LogCommand.Description)]
    public async Task SetLogChannelAsync(
        [SlashCommandParameter(Description = "Log type", Name = "type")] LogChannel logChannel,
        [SlashCommandParameter(Description = "Output channel")] TextChannel? channel = default)
    {
        await configService.SetLogChannelSettingAsync(Context.Guild!.Id, logChannel, channel?.Id);

        await RespondAsync(InteractionCallback.Message($"Set {logChannel.Humanize()} channel to {(channel is null ? "none" : $"<#{channel.Id}>")}"));
    }

    [SubSlashCommand(ConfigModuleMetadata.LogAchievementCommand.Name, ConfigModuleMetadata.LogAchievementCommand.Description)]
    public async Task SetLogAchievementChannelAsync(
        [SlashCommandParameter(Description = "Achievement")] AchievementRole achievement,
        [SlashCommandParameter(Description = "Output channel")] TextChannel? channel = default)
    {

        if (channel is not null)
        {
            var botUser = Context.Guild!.Users[Context.Client.Cache.User!.Id]!;
            var permissions = botUser.GetResolvedChannelPermissions(Context.Guild, channel.Id);

            if (!permissions.HasFlag(Permissions.SendMessages))
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"I don't have permission to send messages in <#{channel.Id}>.",
                    Flags = MessageFlags.Ephemeral
                }));

                return;
            }
        }

        await configService.SetAchievementLogChannelAsync(Context.Guild!.Id, achievement, channel?.Id);

        if (channel is null)
        {
            await RespondAsync(InteractionCallback.Message($"Unset override. {achievement.Humanize()} achievement will log to the default achievement channel."));
        }
        else
        {
            await RespondAsync(InteractionCallback.Message($"Overrode {achievement.Humanize()} achievement to log channel to <#{channel.Id}>"));
        }
    }

    [SubSlashCommand(ConfigModuleMetadata.RoleCommand.Name, ConfigModuleMetadata.RoleCommand.Description)]
    public async Task SetRoleAsync(
        [SlashCommandParameter(Description = "Role", Name = "role")] Role role,
        NetCord.Role selectedRole)
    {
        await configService.SetRoleIdAsync(Context.Guild!.Id, role, selectedRole.Id);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Set {role.Humanize()} role to <@&{selectedRole.Id}>",
            AllowedMentions = AllowedMentionsProperties.None
        }));
    }

    [SubSlashCommand(ConfigModuleMetadata.MessageCommand.Name, ConfigModuleMetadata.MessageCommand.Description)]
    public async Task SetMessageAsync(
        [SlashCommandParameter(Description = "Achievement type", Name = "achievement")] AchievementRole achievementRole,
        [SlashCommandParameter(Description = "Message to send. Use {mention} to mention the user.")] string message)
    {
        await configService.SetAchievementMessageAsync(Context.Guild!.Id, achievementRole, message);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Set {achievementRole.Humanize()} message to:",
            Embeds = [
                new EmbedProperties()
                {
                    Description = message,
                }
            ]
        }));
    }

    [SubSlashCommand(ConfigModuleMetadata.VerifyCommand.Name, ConfigModuleMetadata.VerifyCommand.Description)]
    public async Task SetVerifyRank(Rank minRank)
    {
        await configService.SetVerifyRankAsync(Context.Guild!.Id, minRank);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Set minimum verification rank to {minRank.Humanize()}",
            AllowedMentions = AllowedMentionsProperties.None
        }));
    }

    [Flags]
    public enum SettingType
    {
        Log         = 0b1 << 1,
        Role        = 0b1 << 2,
        Message     = 0b1 << 3,
        [SlashCommandChoice(Name = "Verify Rank")]
        VerifyRank  = 0b1 << 4,
        All = Log | Role | Message | VerifyRank
    }
}

