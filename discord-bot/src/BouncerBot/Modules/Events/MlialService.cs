using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace BouncerBot.Modules.Events;

public interface IMlialService
{
    Summary? GetJournalSummary(string journalPage);
}

public partial class MlialService : IMlialService
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private static HashSet<string> s_UncountableEggs = [
        "3213", // "Green Plaid Egg"
        "3204", // "Blue Argyle Egg"
        "3225", // "Stripy Red Egg"
        "3230", // "Wavy Purple Egg"
        "3216", // "Scalloped Pink Egg"
        "553",  // "Gift Egg"
        "3217", // "Sharing Egg"
        "3212", // "Friendly Egg"
        "3206", // "Caring Egg"
        "3202", // "21K Gold Egg"
    ];

    // showLogSummary("1 day, 13 hours", {"36_343":"7","36_347":"5"}, {"114_bu":"98","114_c":"98"}, {"2991":"18","803":"35","925":"2","211":"2","380":"2","335":"2"}); return false;'
    [GeneratedRegex(@"showLogSummary\(""([^""]+)"",\s*(\{[^{}]*\}),\s*(\{[^{}]*\}),\s*(\{[^{}]*\})\)")]
    private static partial Regex LogSummaryRegex();

    public Summary? GetJournalSummary(string journalPage)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(journalPage);

        var link = doc.DocumentNode
            .SelectSingleNode("//div[contains(@class, 'log_summary')]//a[@onclick]");

        if (link is null)
            return null;

        var onclick = link.GetAttributeValue("onclick", string.Empty);
        var match = LogSummaryRegex().Match(onclick);

        if (!match.Success)
            return null;

        // catchData keys are "<group_id>_<mouse_id>"
        var catchData = JsonSerializer.Deserialize<Dictionary<string, int>>(match.Groups[2].Value, s_jsonOptions) ?? [];
        // baitData keys are "<bait_id>_bu" (hunts used) and "<bait_id>_c" (catches)
        var baitData = JsonSerializer.Deserialize<Dictionary<string, int>>(match.Groups[3].Value, s_jsonOptions) ?? [];
        // lootData keys are item IDs, values are quantities
        var lootData = JsonSerializer.Deserialize<Dictionary<string, int>>(match.Groups[4].Value, s_jsonOptions) ?? [];

        var hunts = baitData
            .Where(kvp => kvp.Key.EndsWith("_bu"))
            .Sum(kvp => kvp.Value);

        var filteredLoot = lootData.Keys.ToHashSet().Except(s_UncountableEggs);
        var filteredLootCount = filteredLoot.Count();
        var messages = new List<string>();

        // Remove one-time Spring Eggs for current rules
        if (lootData.Count != filteredLootCount)
        {
            messages.Add($"I removed {lootData.Count - filteredLootCount} due to one-time Spring Eggs.");
        }

        // Add one for warmonger catches if they don't have the corresponding egg
        var hasWarmongerCatch = catchData.Keys.Any(k => k.EndsWith("_306"));
        var hasWarmongerEgg = lootData.ContainsKey("2317");
        if (hasWarmongerCatch && !hasWarmongerEgg)
        {
            messages.Add("I added one because the Warmonger didn't drop an egg.");
            filteredLootCount++;
        }

        return new Summary
        {
            Since = match.Groups[1].Value,
            Hunts = hunts,
            LootCount = filteredLootCount,
            ModificationMessages = messages,
        };
    }

}
public class Summary
{
    public string Since { get; init; } = string.Empty;
    public int Hunts { get; init; }
    public int LootCount { get; init; }
    public required IReadOnlyList<string> ModificationMessages { get; init; }
}
