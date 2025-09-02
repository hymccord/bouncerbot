using System.Text;

using NetCord;
using NetCord.Rest;

namespace BouncerBot.Modules.Bounce;
internal static class BounceListHelper
{
    public static async Task<IEnumerable<IMessageComponentProperties>> CreateBounceListComponentsAsync(ulong guildId, int page, IBounceService bounceService)
    {
        var results = await bounceService.ListBannedHuntersAsync(guildId, page);

        var totalResults = results.Count();

        var container = new ComponentContainerProperties()
            .WithAccentColor(new NetCord.Color(0x5865F2));

        if (totalResults is 0)
        {
            return [
                container.AddComponents(new TextDisplayProperties("# No banned hunters!"))
            ];
        }

        var title = "# Banned Hunters";

        var sb = new StringBuilder();

        foreach (var bh in results.Take(10))
        {
            sb.AppendLine($"- [{bh.MouseHuntId}](<https://p.mshnt.ca/{bh.MouseHuntId})");
            if (!string.IsNullOrEmpty(bh.Note))
            {
                sb.AppendLine($"  - {bh.Note}");
            }

        }

        return [
            container
                .AddComponents(new TextDisplayProperties(title))
                .AddComponents(new TextDisplayProperties(sb.ToString()))
                .AddComponents(new ActionRowProperties()
                    .AddComponents(
                        new ButtonProperties($"bouncelist:{guildId}:{page - 1}", EmojiProperties.Standard("◀️"), ButtonStyle.Primary)
                            .WithDisabled(page < 1),
                        new ButtonProperties($"bouncelist:{guildId}:{page + 1}", EmojiProperties.Standard("▶️"), ButtonStyle.Primary)
                            .WithDisabled(totalResults < BounceService.ResultsPerPage)
                    )
                )
        ];
    }
}
