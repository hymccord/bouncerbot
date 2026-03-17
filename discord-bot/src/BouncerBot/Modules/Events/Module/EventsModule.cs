using System.Text;
using BouncerBot.Attributes;
using BouncerBot.Rest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Events.Module;

[BouncerBotSlashCommand(EventsModuleMetadata.ModuleName, EventsModuleMetadata.ModuleDescription)]
public partial class EventsModule(
    IOptionsSnapshot<BouncerBotOptions> options,
    IBouncerBotMetrics bouncerBotMetrics,
    IMouseHuntRestClient mouseHuntRestClient,
    IMemoryCache memoryCache,
    MlialService mlialService
)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    private const string CooldownCacheKey = "EventsCooldown";
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
    TimeSpan.FromMinutes(15);
#endif

    [SubSlashCommand(EventsModuleMetadata.MlialCommand.Name, EventsModuleMetadata.MlialCommand.Description)]
    public async Task MlialCommand([SlashCommandParameter(Description = "ID of the hunter to generate a summary for")] uint hunterId,
        [SlashCommandParameter(Description = "The journal page to look at", MinValue = 1, MaxValue = 6)] uint page = 1)
    {
        bouncerBotMetrics.RecordCommand(EventsModuleMetadata.MlialCommand.Name);
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());
        await ModifyResponseAsync(m => new ComponentContainerProperties()
            .WithAccentColor(new Color(options.Value.Colors.Primary))
            .AddTextDisplay("Hold on tight, I'm hunting down that journal summary for you...")
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
            await DeleteResponseAsync();

            var sb = new StringBuilder();

            sb.AppendLine($"""
            Look at this big shot (**{hunterId}**) who has been going at it for **{summary.Since}**.

            In that time, they hunted **{summary.Hunts}** times and secured **{summary.LootCount}** unique loot drops.
            """);

            if (summary.ModificationMessages.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("-# Think I'm wrong!? Here's some modifications I made");
                foreach (var mod in summary.ModificationMessages)
                {
                    sb.AppendLine($"-# • {mod}");
                }
            }

            await Context.Channel.SendMessageAsync(new MessageProperties
            {
                Components = [
                    new ComponentContainerProperties()
                    .WithAccentColor(new(options.Value.Colors.Success))
                    .AddTextDisplay("MLIAL Journal Summary")
                    .AddSeparator()
                    .AddTextDisplay(sb.ToString())
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
