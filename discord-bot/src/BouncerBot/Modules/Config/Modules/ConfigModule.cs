using BouncerBot;
using BouncerBot.Attributes;
using BouncerBot.Modules.RankRole;
using BouncerBot.Services;
using Humanizer;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Config.Modules;

[ManageRolesSlashCommand(ConfigModuleMetadata.ModuleName, ConfigModuleMetadata.ModuleDescription)]
public partial class ConfigModule(
    IOptionsSnapshot<BouncerBotOptions> options,
    IConfigService configService,
    IMouseHuntEmojiService emojiService)
    : ApplicationCommandModule<ApplicationCommandContext>
{

    [SubSlashCommand(ConfigModuleMetadata.LogCommand.Name, ConfigModuleMetadata.LogCommand.Description)]
    [RequireManageGuild<ApplicationCommandContext>()]
    public async Task SetLogChannelAsync(
        [SlashCommandParameter(Description = "Log type", Name = "type")] LogChannel logChannel,
        [SlashCommandParameter(Description = "Output channel")] TextChannel? channel = default)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        await configService.SetLogChannelSettingAsync(Context.Guild!.Id, logChannel, channel?.Id);
        await ModifyResponseAsync(m => new ComponentContainerProperties()
            .WithAccentColor(new(options.Value.Colors.Primary))
            .AddTextDisplay(channel is null? "Unset log channel." : $"Set {logChannel.Humanize()} log channel to <#{channel.Id}>")
            .Build(m));
    }

    [SubSlashCommand(ConfigModuleMetadata.LogAchievementCommand.Name, ConfigModuleMetadata.LogAchievementCommand.Description)]
    [RequireManageGuild<ApplicationCommandContext>()]
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

        await RespondAsync(InteractionCallback.DeferredMessage());
        await configService.SetAchievementLogChannelAsync(Context.Guild!.Id, achievement, channel?.Id);
        await ModifyResponseAsync(m => new ComponentContainerProperties()
            .WithAccentColor(new(options.Value.Colors.Primary))
            .AddTextDisplay(channel is null
                ? $"Unset override. {achievement.Humanize()} achievement will log to the default achievement channel."
                : $"Set {achievement.Humanize()} achievement log channel to <#{channel.Id}>")
            .Build(m));
    }

    [SubSlashCommand(ConfigModuleMetadata.RoleCommand.Name, ConfigModuleMetadata.RoleCommand.Description)]
    [RequireManageGuild<ApplicationCommandContext>()]
    public async Task SetRoleAsync(
        [SlashCommandParameter(Description = "Role", Name = "role")] Role role,
        NetCord.Role selectedRole)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        await configService.SetRoleIdAsync(Context.Guild!.Id, role, selectedRole.Id);
        await ModifyResponseAsync(m => new ComponentContainerProperties()
            .WithAccentColor(new (options.Value.Colors.Primary))
            .AddTextDisplay($"Set {role.Humanize()} role to <@&{selectedRole.Id}>")
            .Build(m));
    }

    [SubSlashCommand(ConfigModuleMetadata.MessageCommand.Name, ConfigModuleMetadata.MessageCommand.Description)]
    [RequireManageGuild<ApplicationCommandContext>()]
    public async Task SetMessageAsync(
        [SlashCommandParameter(Description = "Achievement type", Name = "achievement")] AchievementRole achievementRole,
        [SlashCommandParameter(Description = "Message to send. Use {mention} to mention the user.")] string message)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        await configService.SetAchievementMessageAsync(Context.Guild!.Id, achievementRole, message);
        await ModifyResponseAsync(m =>
            new ComponentContainerProperties()
                .WithAccentColor(new(options.Value.Colors.Primary))
                .AddTextDisplay($"""Set {achievementRole.Humanize()} message""")
                .AddSeparator()
                .AddTextDisplay(message)
                .Build(m)
        );
    }

    [SubSlashCommand(ConfigModuleMetadata.VerifyCommand.Name, ConfigModuleMetadata.VerifyCommand.Description)]
    [RequireManageGuild<ApplicationCommandContext>()]
    public async Task SetVerifyRank(Rank minRank)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        await configService.SetVerifyRankAsync(Context.Guild!.Id, minRank);
        await ModifyResponseAsync(m => new ComponentContainerProperties()
            .WithAccentColor(new(options.Value.Colors.Primary))
            .AddTextDisplay($"Set minimum verification rank to {minRank.Humanize()}")
            .Build(m));
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
