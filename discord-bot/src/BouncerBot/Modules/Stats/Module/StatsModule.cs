using BouncerBot.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Stats.Module;

public class StatsApplicationModule(
    IOptions<BouncerBotOptions> options,
    IMemoryCache memoryCache,
    IStatsService statsService,
    IBouncerBotMetrics bouncerBotMetrics
) : ApplicationCommandModule<ApplicationCommandContext>
{
    [BouncerBotSlashCommand(StatsModuleMetadata.StatsCommand.Name, StatsModuleMetadata.StatsCommand.Description)]
    public async Task GetStatsAsync()
    {
        bouncerBotMetrics.RecordCommand(StatsModuleMetadata.StatsCommand.Name);
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var guildId = Context.Guild!.Id;
        var stats = await statsService.GetGuildStatsAsync(guildId);

        var container = new ComponentContainerProperties()
            .WithAccentColor(new(options.Value.Colors.Primary));

        container.AddComponents(
            new TextDisplayProperties($"""
            ### User Role Stats
            """),

            new ComponentSeparatorProperties()
                .WithDivider()
                .WithSpacing(ComponentSeparatorSpacingSize.Small),

            new TextDisplayProperties($"""
                        **Achievements**
                        - :star:: {stats[Role.Star]}
                        - :crown:: {stats[Role.Crown]}
                        - :white_check_mark:: {stats[Role.Checkmark]}
                        - :egg:: {stats[Role.EggMaster]}
                        - :cookie:: {stats[Role.Achiever]}
                        """),

            new ComponentSeparatorProperties()
                .WithDivider()
                .WithSpacing(ComponentSeparatorSpacingSize.Small),

            new TextDisplayProperties($"""
                **Masteries**
                - {options.Value.Emojis.Arcane}: {stats[Role.ArcaneMaster]}
                - {options.Value.Emojis.Draconic}: {stats[Role.DraconicMaster]}
                - {options.Value.Emojis.Forgotten}: {stats[Role.ForgottenMaster]}
                - {options.Value.Emojis.Hydro}: {stats[Role.HydroMaster]}
                - {options.Value.Emojis.Law}: {stats[Role.LawMaster]}
                - {options.Value.Emojis.Physical}: {stats[Role.PhysicalMaster]}
                - {options.Value.Emojis.Rift}: {stats[Role.RiftMaster]}
                - {options.Value.Emojis.Shadow}: {stats[Role.ShadowMaster]}
                - {options.Value.Emojis.Tactical}: {stats[Role.TacticalMaster]}
                - {options.Value.Emojis.Multi}: {stats[Role.MultiMaster]}
                """)
        );

        await ModifyResponseAsync(m =>
        {
            m.Components = [container];
            m.Flags = MessageFlags.IsComponentsV2 | MessageFlags.Ephemeral;
        });
    }
}
