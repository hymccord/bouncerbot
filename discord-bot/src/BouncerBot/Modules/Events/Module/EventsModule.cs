using System.Text;
using BouncerBot.Attributes;
using BouncerBot.Rest;
using Humanizer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Scriban;

namespace BouncerBot.Modules.Events.Module;

[BouncerBotSlashCommand(EventsModuleMetadata.ModuleName, EventsModuleMetadata.ModuleDescription)]
public class EventsModule(
    IOptionsSnapshot<BouncerBotOptions> options,
    IBouncerBotMetrics bouncerBotMetrics,
    IMouseHuntRestClient mouseHuntRestClient,
    IMemoryCache memoryCache,
    IMlialService mlialService
)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    private const string CooldownCacheKey = "EventsCooldown";
    private static readonly string[] s_summaryPhrases =
    [
        "Look at this big shot, **{{ discord_name }}** who has been going at it for **{{ since }}**.",
        "Well, well, well — **{{ discord_name }}** has been grinding away for **{{ since }}**.",
        "Feast your eyes on **{{ discord_name }}**, who's been on the hunt for **{{ since }}**.",
        "Would you look at that? **{{ discord_name }}** has been keeping busy for **{{ since }}**.",
        "Here's today's overachiever: **{{ discord_name }}** with **{{ since }}** of journal-worthy effort.",
        "Oh, this is good — **{{ discord_name }}** has been at it for **{{ since }}**.",
        "You wanted a summary, and **{{ discord_name }}** delivered **{{ since }}** worth of hunting.",
        "Now that's some dedication: **{{ discord_name }}** has been chasing mice for **{{ since }}**.",
    ];
    private static readonly string[] s_statsPhrases =
    [
        "In that time, they secured **{{ loot_count }}** unique loot drops in **{{ hunt_count }}** hunts.",
        "That's **{{ loot_count }}** unique loot drops across **{{ hunt_count }}** hunts.",
        "They've racked up **{{ loot_count }}** unique loot drops over **{{ hunt_count }}** hunts.",
        "**{{ loot_count }}** unique loot drops to show for **{{ hunt_count }}** hunts of effort.",
        "A haul of **{{ loot_count }}** unique loot drops from **{{ hunt_count }}** hunts.",
        "**{{ loot_count }}** unique loot drops. **{{ hunt_count }}** hunts. Not bad.",
        "**{{ loot_count }}** unique loot drops earned, spread across **{{ hunt_count }}** hunts.",
        "**{{ loot_count }}** unique loot drops collected across **{{ hunt_count }}** hunts."
    ];
    private static readonly TimeSpan s_shortCooldown =
#if DEBUG
        TimeSpan.FromSeconds(1);
#else
        TimeSpan.FromSeconds(15);
#endif
    private static readonly TimeSpan s_longCooldown =
#if DEBUG
    TimeSpan.FromSeconds(1);
#else
    TimeSpan.FromMinutes(60);
#endif

    [SubSlashCommand(EventsModuleMetadata.MlialCommand.Name, EventsModuleMetadata.MlialCommand.Description)]
    public async Task MlialCommand([SlashCommandParameter(Description = "ID of the hunter to generate a summary for", MinValue = 0, MaxValue = 20_000_000)] uint hunterId,
        [SlashCommandParameter(Description = "The journal page to look at", MinValue = 1, MaxValue = 6)] uint page = 1)
    {
        bouncerBotMetrics.RecordCommand(EventsModuleMetadata.MlialCommand.Name);
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());
        await ModifyResponseAsync(m => new ComponentContainerProperties()
            .WithAccentColor(new Color(options.Value.Colors.Primary))
            .AddTextDisplay($"{options.Value.Emojis.Loading} Hold on tight, I'm hunting down that journal summary for you...")
            .Build(m)
        );

        var journal = await mouseHuntRestClient.GetUserJournalPage(hunterId, page);
        var summary = mlialService.GetJournalSummary(journal);

        if (IsUserOnCooldown(hunterId, out var resetTime))
        {
            await ModifyResponseAsync(m => new ComponentContainerProperties()
                .WithAccentColor(new Color(options.Value.Colors.Warning))
                .AddTextDisplay($"""
                I can only do things so fast! Let me catch my breath.

                -# Hint: That hunter ID is on cooldown. Try it again in <t:{resetTime.ToUnixTimeSeconds()}:R>.
                """)
                .Build(m)
            );

            return;
        }

        if (summary == null)
        {
            await ModifyResponseAsync(m => new ComponentContainerProperties()
                .WithAccentColor(new(options.Value.Colors.Warning))
                .AddTextDisplay($"Sorry, I looked high and low — really only at page {page} — but couldn't find a journal summary!")
                .Build(m)
            );

            SetUserOnCooldown(hunterId, s_shortCooldown);
        }
        else
        {
            var profileInfo = await mouseHuntRestClient.GetUserProfileSimpleInfoAsync(hunterId);
            var titles = await mouseHuntRestClient.GetTitlesAsync();
            await DeleteResponseAsync();

            var sb = new StringBuilder();
            var randomSummaryPhrase = s_summaryPhrases[Random.Shared.Next(s_summaryPhrases.Length)];
            var randomStatsPhrase = s_statsPhrases[Random.Shared.Next(s_statsPhrases.Length)];
            var templateModel = new
            {
                discord_name = Format.Escape(Context.User.GlobalName ?? Context.User.Username),
                hunter_name = Format.Escape(profileInfo.Name.Trim()),
                title = titles.Single(t => t.TitleId == profileInfo.TitleId).Name.Humanize(),
                hunter_id = hunterId,
                since = summary.Since,
                loot_count = summary.LootCount,
                hunt_count = summary.Hunts,
                mods = summary.ModificationMessages
            };

            var template = $$$"""
                {{{randomSummaryPhrase}}}

                {{{randomStatsPhrase}}}
                {{- if array.size(mods) > 0 }}

                -# Think I'm wrong!? Here's some modifications I made
                {{- for mod in mods }}
                -# • {{mod}}
                {{- end }}
                {{- end}}

                -# <https://p.mshnt.ca/{{hunter_id}}>
                -# {{hunter_name}},{{title}},{{since}},{{loot_count}},{{hunt_count}}
                """;
            var rendered = Template.Parse(template).Render(templateModel);

            await Context.Channel.SendMessageAsync(new MessageProperties
            {
                Components = [
                    new ComponentContainerProperties()
                    .WithAccentColor(new(options.Value.Colors.Success))
                    .AddTextDisplay($"[MLIAL Journal Summary](<https://p.mshnt.ca/{hunterId}>)")
                    .AddSeparator()
                    .AddTextDisplay(rendered)
                ],
                Flags = MessageFlags.IsComponentsV2
            });

            SetUserOnCooldown(hunterId, s_longCooldown);
        }
    }

    private void SetUserOnCooldown(uint id, TimeSpan duration)
    {
        memoryCache.Set($"{CooldownCacheKey}-{id}", DateTimeOffset.UtcNow + duration, TimeSpan.FromMinutes(1));
    }

    private bool IsUserOnCooldown(uint id, out DateTimeOffset resetTime)
        => memoryCache.TryGetValue($"{CooldownCacheKey}-{id}", out resetTime) && resetTime > DateTimeOffset.UtcNow;
}
