using System.Text;

using BouncerBot.Attributes;
using BouncerBot.Modules.Config;

using Humanizer;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Variables.Modules;

[SlashCommand("config", "Manage bot configuration")]
[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
[RequireGuildContext<ApplicationCommandContext>]
public class ConfigModule(
    IOptionsSnapshot<Options> options,
    IConfigService configService)
    : ApplicationCommandModule<ApplicationCommandContext>
{

    [SubSlashCommand("log", "Set channel where specified events go. Leave empty to clear channel.")]
    public async Task SetLogChannelAsync(
        [SlashCommandParameter(Description = "Log type", Name = "type")] LogChannel logChannel,
        [SlashCommandParameter(Description = "Output channel")] Channel? channel = default)
    {
        await configService.SetLogChannelSettingAsync(Context.Guild!.Id, logChannel, channel?.Id);

        await RespondAsync(InteractionCallback.Message($"Set {logChannel.Humanize()} channel to {(channel is null ? "none" : $"<#{channel.Id}>")}"));
    }

    [SubSlashCommand("role", "Set role for various bot operations")]
    public async Task SetRoleAsync(
        [SlashCommandParameter(Description = "Role", Name = "role")] Role role,
        NetCord.Role selectedRole)
    {
        await configService.SetRoleSettingAsync(Context.Guild!.Id, role, selectedRole.Id);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Set {role.Humanize()} role to <@&{selectedRole.Id}>",
            AllowedMentions = AllowedMentionsProperties.None
        }));
    }

    [SubSlashCommand("message", "Set message for specified achievement type. Use {mention} to mention user.")]
    public async Task SetMessageAsync(
        [SlashCommandParameter(Description = "Achievement type", Name = "achievement")] AchievementRole achievementRole,
        [SlashCommandParameter(Description = "Message to send.")] string message)
    {
        await configService.SetMessageSettingAsync(Context.Guild!.Id, achievementRole, message);

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

    [SubSlashCommand("link", "Set minimum rank required for to successfully use /link command.")]
    public async Task SetVerifyRank(Rank minRank)
    {
        await configService.SetVerifyRankAsync(Context.Guild!.Id, minRank);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Set verification rank to {minRank.Humanize()}",
            AllowedMentions = AllowedMentionsProperties.None
        }));
    }

    [SubSlashCommand("list", "List settings")]
    public async Task ViewConfigAsync(SettingType? setting = SettingType.All)
    {
        var config = await configService.GetGuildConfigAsync(Context.Guild!.Id);

        var sb = new StringBuilder();

        if (setting?.HasFlag(SettingType.Log) ?? true)
        {
            sb.AppendLine($"""
                ## Log Channels:
                - General: {(config.LogSettings?.LogId is null ? "None" : $"<#{config.LogSettings.LogId}>")}
                - Achievement: {(config.LogSettings?.FlexId is null ? "None" : $"<#{config.LogSettings.FlexId}>")}
                - Egg Master: {(config.LogSettings?.EggMasterId is null ? "None" : $"<#{config.LogSettings.EggMasterId}>")}
                - Verification: {(config.LogSettings?.VerificationId is null ? "None" : $"<#{config.LogSettings.VerificationId}>")}
                """);
        }

        if (setting?.HasFlag(SettingType.Role) ?? true)
        {
            sb.AppendLine($"""
                ## Roles:
                - Verified: {(config.RoleSettings?.VerifiedId is null ? "None" : $"<@&{config.RoleSettings?.VerifiedId}>")}

                ### Achievement Roles
                - :star:: {(config.RoleSettings?.StarId is null ? "None" : $"<@&{config.RoleSettings?.StarId}>")}
                - :crown:: {(config.RoleSettings?.CrownId is null ? "None" : $"<@&{config.RoleSettings?.CrownId}>")}
                - :white_check_mark:: {(config.RoleSettings?.CheckmarkId is null ? "None" : $"<@&{config.RoleSettings?.CheckmarkId}>")}
                - :egg:: {(config.RoleSettings?.EggMasterId is null ? "None" : $"<@&{config.RoleSettings?.EggMasterId}>")}
                - :cookie:: {(config.RoleSettings?.AchieverId is null ? "None" : $"<@&{config.RoleSettings?.AchieverId}>")}

                ### Mastery Roles
                - {options.Value.Emojis.Arcane}: {(config.RoleSettings?.ArcaneMasterId is null ? "None" : $"<@&{config.RoleSettings?.ArcaneMasterId}>")}
                - {options.Value.Emojis.Draconic}: {(config.RoleSettings?.DraconicMasterId is null ? "None" : $"<@&{config.RoleSettings?.DraconicMasterId}>")}
                - {options.Value.Emojis.Forgotten}: {(config.RoleSettings?.ForgottenMasterId is null ? "None" : $"<@&{config.RoleSettings?.ForgottenMasterId}>")}
                - {options.Value.Emojis.Hydro}: {(config.RoleSettings?.HydroMasterId is null ? "None" : $"<@&{config.RoleSettings?.HydroMasterId}>")}
                - {options.Value.Emojis.Law}: {(config.RoleSettings?.LawMasterId is null ? "None" : $"<@&{config.RoleSettings?.LawMasterId}>")}
                - {options.Value.Emojis.Physical}: {(config.RoleSettings?.PhysicalMasterId is null ? "None" : $"<@&{config.RoleSettings?.PhysicalMasterId}>")}
                - {options.Value.Emojis.Rift}: {(config.RoleSettings?.RiftMasterId is null ? "None" : $"<@&{config.RoleSettings?.RiftMasterId}>")}
                - {options.Value.Emojis.Shadow}: {(config.RoleSettings?.ShadowMasterId is null ? "None" : $"<@&{config.RoleSettings?.ShadowMasterId}>")}
                - {options.Value.Emojis.Tactical}: {(config.RoleSettings?.TacticalMasterId is null ? "None" : $"<@&{config.RoleSettings?.TacticalMasterId}>")}
                - {options.Value.Emojis.Multi}: {(config.RoleSettings?.MultiMasterId is null ? "None" : $"<@&{config.RoleSettings?.MultiMasterId}>")}
                """);
        }

        if (setting?.HasFlag(SettingType.Message) ?? true)
        {
            sb.AppendLine($"""
                ## Messages:
                - :star:: {config.AchievementMessages?.Star ?? ""}
                - :crown:: {config.AchievementMessages?.Crown ?? ""}
                - :white_check_mark:: {config.AchievementMessages?.Checkmark ?? ""}
                - :egg:: {config.AchievementMessages?.EggMaster ?? ""}
                        
                - {options.Value.Emojis.Arcane}: {config.AchievementMessages?.ArcaneMaster ?? ""}
                - {options.Value.Emojis.Draconic}: {config.AchievementMessages?.DraconicMaster ?? ""}
                - {options.Value.Emojis.Forgotten}: {config.AchievementMessages?.ForgottenMaster ?? ""}
                - {options.Value.Emojis.Hydro}: {config.AchievementMessages?.HydroMaster ?? ""}
                - {options.Value.Emojis.Law}: {config.AchievementMessages?.LawMaster ?? ""}
                - {options.Value.Emojis.Physical}: {config.AchievementMessages?.PhysicalMaster ?? ""}
                - {options.Value.Emojis.Rift}: {config.AchievementMessages?.RiftMaster ?? ""}
                - {options.Value.Emojis.Shadow}: {config.AchievementMessages?.ShadowMaster ?? ""}
                - {options.Value.Emojis.Tactical}: {config.AchievementMessages?.TacticalMaster ?? ""}
                - {options.Value.Emojis.Multi}: {config.AchievementMessages?.MultiMaster ?? ""}
                """);
        }

        if (setting?.HasFlag(SettingType.VerifyRank) ?? true)
        {
            sb.AppendLine($"""
                ## Verification:
                - Minimum Rank: {config.VerifySettings?.MinimumRank.Humanize() ?? Rank.Novice.Humanize()}
                """);
        }

        var embed = new EmbedProperties()
        {
            Title = "Current Configuration",
            Description = sb.ToString(),
        };

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Embeds = [embed],
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

