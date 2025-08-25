using Microsoft.Extensions.Options;

namespace BouncerBot.Services;

public interface IMouseHuntEmojiService
{
    string GetPowerTypeEmoji(PowerType powerType);
    string? GetTitleShieldEmoji(Rank rank);
}

internal class MouseHuntEmojiService(
    IOptions<BouncerBotOptions> options
) : IMouseHuntEmojiService
{
    public string GetPowerTypeEmoji(PowerType powerType)
    {
        return powerType switch
        {
            PowerType.None => options.Value.Emojis.Multi,
            PowerType.Arcane => options.Value.Emojis.Arcane,
            PowerType.Draconic => options.Value.Emojis.Draconic,
            PowerType.Forgotten => options.Value.Emojis.Forgotten,
            PowerType.Hydro => options.Value.Emojis.Hydro,
            PowerType.Law => options.Value.Emojis.Law,
            PowerType.Physical => options.Value.Emojis.Physical,
            PowerType.Rift => options.Value.Emojis.Rift,
            PowerType.Shadow => options.Value.Emojis.Shadow,
            PowerType.Tactical => options.Value.Emojis.Tactical,
            PowerType.Multi => options.Value.Emojis.Multi,
            _ => throw new NotImplementedException(),
        };
    }

    public string? GetTitleShieldEmoji(Rank rank)
    {
        return rank switch
        {
            Rank.Novice => options.Value.Emojis.Titles.Novice,
            Rank.Recruit => options.Value.Emojis.Titles.Recruit,
            Rank.Apprentice => options.Value.Emojis.Titles.Apprentice,
            Rank.Initiate => options.Value.Emojis.Titles.Initiate,
            Rank.Journeyman => options.Value.Emojis.Titles.Journeyman,
            Rank.Master => options.Value.Emojis.Titles.Master,
            Rank.Grandmaster => options.Value.Emojis.Titles.Grandmaster,
            Rank.Legendary => options.Value.Emojis.Titles.Legendary,
            Rank.Hero => options.Value.Emojis.Titles.Hero,
            Rank.Knight => options.Value.Emojis.Titles.Knight,
            Rank.Lord => options.Value.Emojis.Titles.Lord,
            Rank.Baron => options.Value.Emojis.Titles.Baron,
            Rank.Count => options.Value.Emojis.Titles.Count,
            Rank.Duke => options.Value.Emojis.Titles.Duke,
            Rank.GrandDuke => options.Value.Emojis.Titles.GrandDuke,
            Rank.Archduke => options.Value.Emojis.Titles.Archduke,
            Rank.Viceroy => options.Value.Emojis.Titles.Viceroy,
            Rank.Elder => options.Value.Emojis.Titles.Elder,
            Rank.Sage => options.Value.Emojis.Titles.Sage,
            Rank.Fabled => options.Value.Emojis.Titles.Fabled,
            _ => throw new NotImplementedException(),
        };
    }
}
