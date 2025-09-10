using Microsoft.Extensions.Options;
using NetCord.Rest;

namespace BouncerBot.Services;

public interface IReactionService
{
    Task AddAchievementReactionAsync(ulong channelId, ulong messageId, AchievementRole achievement, CancellationToken cancellationToken = default);
}

internal class ReactionService(
    IOptions<BouncerBotOptions> options,
    RestClient rest
    ) : IReactionService
{
    public async Task AddAchievementReactionAsync(ulong channelId, ulong messageId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        var reaction = GetAppAchievmentEmoji(achievement);
        if (reaction is null)
        {
            return;
        }

        await rest.AddMessageReactionAsync(channelId, messageId, reaction, cancellationToken: cancellationToken);
    }

    //public async Task AddTitleRankReactionAsync(ulong channelId, ulong messageId, Rank rank, CancellationToken cancellationToken = default)
    //{
    //    ReactionEmojiProperties reaction;
    //    var (name, id) = GetTitleRankEmoji(rank);
    //    await rest.AddMessageReactionAsync(channelId, messageId, new ReactionEmojiProperties(name, id))
    //}

    private ReactionEmojiProperties? GetAppAchievmentEmoji(AchievementRole achievement)
    {

        var emoji = achievement switch
        {
            AchievementRole.Star => "⭐",
            AchievementRole.Crown => "👑",
            AchievementRole.Checkmark => "✅",
            AchievementRole.EggMaster => "🥚",
            _ => null
        };

        if (emoji is not null)
        {
            return new(emoji);
        }

        // <:name:id>
        var rawEmoji = achievement switch
        {
            AchievementRole.Fabled => options.Value.Emojis.Titles.Fabled,
            AchievementRole.ArcaneMaster => options.Value.Emojis.Arcane,
            AchievementRole.DraconicMaster => options.Value.Emojis.Draconic,
            AchievementRole.ForgottenMaster => options.Value.Emojis.Forgotten,
            AchievementRole.HydroMaster => options.Value.Emojis.Hydro,
            AchievementRole.LawMaster => options.Value.Emojis.Law,
            AchievementRole.PhysicalMaster => options.Value.Emojis.Physical,
            AchievementRole.RiftMaster => options.Value.Emojis.Rift,
            AchievementRole.ShadowMaster => options.Value.Emojis.Shadow,
            AchievementRole.TacticalMaster => options.Value.Emojis.Tactical,
            AchievementRole.MultiMaster => options.Value.Emojis.Multi,
            _ => throw new ArgumentOutOfRangeException(nameof(achievement), achievement, null),
        };

        if (rawEmoji is null)
        {
            return null;
        }

        var parts = rawEmoji.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return new(parts[1].TrimStart('<'), ulong.Parse(parts[2].TrimEnd('>')));
    }
}
