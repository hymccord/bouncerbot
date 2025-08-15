using System.Text;
using Humanizer;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Config.Modules;
public partial class ConfigModule
{
    [SubSlashCommand(ConfigModuleMetadata.ListCommand.Name, ConfigModuleMetadata.ListCommand.Description)]
    public async Task ViewConfigAsync(SettingType? setting = SettingType.All, bool? ephemeral = true)
    {
        var guildId = Context.Guild!.Id;

        var components = new List<ComponentContainerProperties>();

        if (setting?.HasFlag(SettingType.Log) ?? true)
        {
            var container = new ComponentContainerProperties()
                .WithAccentColor(new(options.Value.Colors.Primary));
            var sb = new StringBuilder();
            foreach (var achievement in Enum.GetValues<AchievementRole>())
            {
                var channel = await configService.GetAchievementLogChannelAsync(guildId, achievement);
                if (channel.HasValue)
                {
                    sb.AppendLine($"- {achievement.Humanize()}: <#{channel.Value}>");
                }
            }

            container.AddComponents(
                new TextDisplayProperties($"""
                    ## Log Channels:
                    - General: {(await GetChannelMentionAsync(configService.GetLogChannelAsync(guildId, LogChannel.General)))}
                    - Achievement: {(await GetChannelMentionAsync(configService.GetLogChannelAsync(guildId, LogChannel.Achievement)))}
                    - Verification: {(await GetChannelMentionAsync(configService.GetLogChannelAsync(guildId, LogChannel.Verification)))}
                    """),
                new ComponentSeparatorProperties()
                    .WithDivider()
                    .WithSpacing(ComponentSeparatorSpacingSize.Small),
                new TextDisplayProperties($"""
                    ### Achievement Log Channels Override:
                    {sb}
                    """)
            );

            components.Add(container);
        }

        if (setting?.HasFlag(SettingType.Role) ?? true)
        {
            components.Add(new ComponentContainerProperties()
                .WithAccentColor(new(options.Value.Colors.Primary))
                .AddComponents(
                    new TextDisplayProperties($"""
                        ## Roles:
                        - Verified: {await GetRoleMentionAsync(Role.Verified)}
                        """),

                new ComponentSeparatorProperties()
                    .WithDivider()
                    .WithSpacing(ComponentSeparatorSpacingSize.Small),

                new TextDisplayProperties($"""
                    ### Achievement Roles
                    - :star:: {await GetRoleMentionAsync(Role.Star)}
                    - :crown:: {await GetRoleMentionAsync(Role.Crown)}
                    - :white_check_mark:: {await GetRoleMentionAsync(Role.Checkmark)}
                    - :egg:: {await GetRoleMentionAsync(Role.EggMaster)}
                    - :cookie:: {await GetRoleMentionAsync(Role.Achiever)}
                    """),

                new ComponentSeparatorProperties()
                    .WithDivider()
                    .WithSpacing(ComponentSeparatorSpacingSize.Small),

                new TextDisplayProperties($"""
                    ### Mastery Roles
                    - {options.Value.Emojis.Arcane}: {await GetRoleMentionAsync(Role.ArcaneMaster)}
                    - {options.Value.Emojis.Draconic}: {await GetRoleMentionAsync(Role.DraconicMaster)}
                    - {options.Value.Emojis.Forgotten}: {await GetRoleMentionAsync(Role.ForgottenMaster)}
                    - {options.Value.Emojis.Hydro}: {await GetRoleMentionAsync(Role.HydroMaster)}
                    - {options.Value.Emojis.Law}: {await GetRoleMentionAsync(Role.LawMaster)}
                    - {options.Value.Emojis.Physical}: {await GetRoleMentionAsync(Role.PhysicalMaster)}
                    - {options.Value.Emojis.Rift}: {await GetRoleMentionAsync(Role.RiftMaster)}
                    - {options.Value.Emojis.Shadow}: {await GetRoleMentionAsync(Role.ShadowMaster)}
                    - {options.Value.Emojis.Tactical}: {await GetRoleMentionAsync(Role.TacticalMaster)}
                    - {options.Value.Emojis.Multi}: {await GetRoleMentionAsync(Role.MultiMaster)}
                    """)
                ));
        }

        if (setting?.HasFlag(SettingType.Message) ?? true)
        {
            components.Add(new ComponentContainerProperties()
                .WithAccentColor(new(options.Value.Colors.Primary))
                .AddComponents(
                    new TextDisplayProperties("## Messages"),
                    new ComponentSeparatorProperties()
                        .WithDivider()
                        .WithSpacing(ComponentSeparatorSpacingSize.Small),
                    new TextDisplayProperties($"""
                        ### Achievement Messages
                        - :star:: {(await GetAchievementMessageAsync(AchievementRole.Star))}
                        - :crown:: {(await GetAchievementMessageAsync(AchievementRole.Crown))}
                        - :white_check_mark:: {(await GetAchievementMessageAsync(AchievementRole.Checkmark))}
                        - :egg:: {(await GetAchievementMessageAsync(AchievementRole.EggMaster))}
                        """),
                        new ComponentSeparatorProperties()
                            .WithDivider()
                            .WithSpacing(ComponentSeparatorSpacingSize.Small),
                        new TextDisplayProperties($"""
                        ### Mastery Messages
                        - {options.Value.Emojis.Arcane}: {(await GetAchievementMessageAsync(AchievementRole.ArcaneMaster))}
                        - {options.Value.Emojis.Draconic}: {(await GetAchievementMessageAsync(AchievementRole.DraconicMaster))}
                        - {options.Value.Emojis.Forgotten}: {(await GetAchievementMessageAsync(AchievementRole.ForgottenMaster))}
                        - {options.Value.Emojis.Hydro}: {(await GetAchievementMessageAsync(AchievementRole.HydroMaster))}
                        - {options.Value.Emojis.Law}: {(await GetAchievementMessageAsync(AchievementRole.LawMaster))}
                        - {options.Value.Emojis.Physical}: {(await GetAchievementMessageAsync(AchievementRole.PhysicalMaster))}
                        - {options.Value.Emojis.Rift}: {(await GetAchievementMessageAsync(AchievementRole.RiftMaster))}
                        - {options.Value.Emojis.Shadow}: {(await GetAchievementMessageAsync(AchievementRole.ShadowMaster))}
                        - {options.Value.Emojis.Tactical}: {(await GetAchievementMessageAsync(AchievementRole.TacticalMaster))}
                        - {options.Value.Emojis.Multi}: {(await GetAchievementMessageAsync(AchievementRole.MultiMaster))}
                        """)
                ));
        }

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Components = components,
            Flags = MessageFlags.IsComponentsV2 | ((ephemeral ?? true) ? MessageFlags.Ephemeral : 0),
            AllowedMentions = AllowedMentionsProperties.None
        }));

        async Task<string> GetChannelMentionAsync(Task<ulong?> channelTask)
        {
            var channelId = await channelTask;
            return channelId.HasValue ? $"<#{channelId.Value}>" : "None";
        }

        async Task<string> GetRoleMentionAsync(Role role)
        {
            var roleId = await configService.GetRoleIdAsync(guildId, role);
            return roleId.HasValue ? $"<@&{roleId.Value}>" : string.Empty;
        }

        async Task<string> GetAchievementMessageAsync(AchievementRole achievementRole)
        {
            var message = await configService.GetAchievementMessageAsync(guildId, achievementRole);
            return message ?? string.Empty;
        }
    }
}
