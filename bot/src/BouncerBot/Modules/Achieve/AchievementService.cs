using BouncerBot.Rest;
using BouncerBot.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BouncerBot.Modules.Achieve;

public interface IAchievementService
{
    Task<bool> HasAchievementAsync(uint mhId, AchievementRole achievement, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides methods to determine if a user has achieved specific milestones in the MouseHunt game.
/// </summary>
/// <remarks>The <see cref="AchievementService"/> class interacts with the MouseHunt API to verify various
/// achievements for a user, such as checkmarks, egg mastery, crowns, stars, and power type mastery. It utilizes the
/// <see cref="MouseHuntRestClient"/> and <see cref="IMouseRipService"/> to retrieve necessary data.</remarks>
public class AchievementService(
    IOptionsSnapshot<BouncerBotOptions> options,
    IMouseHuntRestClient mouseHuntClient,
    IMouseRipService mouseRipService) : IAchievementService
{
    public async Task<bool> HasAchievementAsync(uint mhId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (options.Value.Debug.DisableAchievementCheck)
        {
            return true;
        }

        return achievement switch
        {
            AchievementRole.Checkmark => await HasCheckmarkAsync(mhId, cancellationToken),
            AchievementRole.EggMaster => await HasEggMasterAsync(mhId, cancellationToken),
            AchievementRole.Crown => await HasCrownAsync(mhId, cancellationToken),
            AchievementRole.Star => await HasStarAsync(mhId, cancellationToken),
            AchievementRole.Fabled => await HasFabledRank(mhId, cancellationToken),
            AchievementRole.ArcaneMaster => await HasPowerTypeMastery(mhId, PowerType.Arcane, cancellationToken),
            AchievementRole.DraconicMaster => await HasPowerTypeMastery(mhId, PowerType.Draconic, cancellationToken),
            AchievementRole.ForgottenMaster => await HasPowerTypeMastery(mhId, PowerType.Forgotten, cancellationToken),
            AchievementRole.HydroMaster => await HasPowerTypeMastery(mhId, PowerType.Hydro, cancellationToken),
            AchievementRole.LawMaster => await HasPowerTypeMastery(mhId, PowerType.Law, cancellationToken),
            AchievementRole.PhysicalMaster => await HasPowerTypeMastery(mhId, PowerType.Physical, cancellationToken),
            AchievementRole.RiftMaster => await HasPowerTypeMastery(mhId, PowerType.Rift, cancellationToken),
            AchievementRole.ShadowMaster => await HasPowerTypeMastery(mhId, PowerType.Shadow, cancellationToken),
            AchievementRole.TacticalMaster => await HasPowerTypeMastery(mhId, PowerType.Tactical, cancellationToken),
            AchievementRole.MultiMaster => await HasPowerTypeMastery(mhId, PowerType.Multi, cancellationToken),
            _ => throw new NotImplementedException($"I don't know what that achievement is yet!")
        };
    }

    // If you've got all five checkmarks (Weapons, Bases, Maps, Collectibles, Skins)
    private async Task<bool> HasCheckmarkAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserProfileItems(mhId, cancellationToken);

        return data.Categories.All(c => c.IsComplete ?? false);
    }

    // Egg Master Achievement: Collect all SEH eggs
    private async Task<bool> HasEggMasterAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        return await mouseHuntClient.IsEggMaster(mhId, cancellationToken);
    }

    // If you've got all five checkmarks (Weapons, Bases, Maps, Collectibles, Skins)
    private async Task<bool> HasCrownAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserMiceAsync(mhId, cancellationToken);

        return data.Mice
            // Filter out Leppy and Mobster
            .Where(m => m.MouseId != 113 && m.MouseId != 128)
            .All(m => m.NumCatches >= 10);
    }

    // If you've got three stars in all locations (meaning you've caught all mice available while in that location)
    private async Task<bool> HasStarAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserLocationStatsAsync(mhId, cancellationToken);

        return data.Categories.All(c => c.IsComplete ?? false);
    }

    private async Task<bool> HasFabledRank(uint mhId, CancellationToken cancellationToken = default)
    {
        var titles = await mouseHuntClient.GetTitlesAsync(cancellationToken)
            ?? throw new Exception("Failed to fetch titles from MouseHunt API");

        var userTitle = await mouseHuntClient.GetUserTitleAsync(mhId, cancellationToken)
            ?? throw new Exception("Failed to fetch user title from MouseHunt API");

        return titles.Single(t => t.TitleId == userTitle.TitleId).Name == Rank.Fabled;
    }

    // Every mouse of given powertype has 100 catches
    private async Task<bool> HasPowerTypeMastery(uint mhId, PowerType powerType, CancellationToken cancellationToken = default)
    {
        var miceClassifications = await GetMiceClassifications();
        var userMice = await mouseHuntClient.GetUserMiceAsync(mhId, cancellationToken);

        return userMice.Mice
            .Where(m => miceClassifications[m.MouseId] == powerType)
            .All(m => m.NumCatches >= 100);
    }

    private async Task<Dictionary<uint, PowerType>> GetMiceClassifications()
    {
        var data = await mouseRipService.GetAllMiceAsync() ?? throw new InvalidOperationException("Failed to retrieve mice from api.mouse.rip.");

        var mice = new Dictionary<uint, PowerType>();
        foreach (var mouse in data)
        {
            // Ignore Leppy, Mobster and Event Mice
            if (mouse.Id == 113 || mouse.Id == 128 || mouse.Group == "Event Mice")
            {
                mice[mouse.Id] = PowerType.None;
                continue;
            }

            if (mouse.Effectivenesses is null)
            {
                continue;
            }

            mouse.Effectivenesses.Remove(MouseRipEffectivenesses.Power);

            var maxEff = mouse.Effectivenesses.Values.Max();
            var keysWithHighestValue = mouse.Effectivenesses.Keys
                .Where(k => mouse.Effectivenesses[k] == maxEff)
                .ToHashSet();

            if (keysWithHighestValue.Count > 1)
            {
                mice[mouse.Id] = PowerType.Multi;
            }
            else
            {
                mice[mouse.Id] = keysWithHighestValue.Single() switch
                {
                    MouseRipEffectivenesses.Arcane => PowerType.Arcane,
                    MouseRipEffectivenesses.Draconic => PowerType.Draconic,
                    MouseRipEffectivenesses.Forgotten => PowerType.Forgotten,
                    MouseRipEffectivenesses.Hydro => PowerType.Hydro,
                    MouseRipEffectivenesses.Law => PowerType.Law,
                    MouseRipEffectivenesses.Physical => PowerType.Physical,
                    MouseRipEffectivenesses.Rift => PowerType.Rift,
                    MouseRipEffectivenesses.Shadow => PowerType.Shadow,
                    MouseRipEffectivenesses.Tactical => PowerType.Tactical,
                    _ => throw new ArgumentOutOfRangeException($"Unknown effectiveness: {keysWithHighestValue.Single()} for mouse {mouse.Id} ({mouse.Name})"),
                };
            }
        }

        return mice;
    }
}
