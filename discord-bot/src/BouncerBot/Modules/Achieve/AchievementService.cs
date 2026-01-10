using BouncerBot.Rest;
using BouncerBot.Services;
using Microsoft.Extensions.Options;

namespace BouncerBot.Modules.Achieve;

public interface IAchievementService
{
    Task<AchievementProgress> HasAchievementAsync(uint mhId, AchievementRole achievement, CancellationToken cancellationToken = default);
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
    public async Task<AchievementProgress> HasAchievementAsync(uint mhId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        if (options.Value.Debug.DisableAchievementCheck)
        {
            // Return a completed progress object when checks are disabled
            return achievement switch
            {
                AchievementRole.Checkmark => new CheckmarkProgress { IsComplete = true, CompletedCategories = 1, TotalCategories = 1, IncompleteCategories = [] },
                AchievementRole.Crown => new CrownProgress { IsComplete = true, MiceWithCrown = 1, TotalMice = 1, MissingMice = [] },
                AchievementRole.Star => new StarProgress { IsComplete = true, CompletedLocations = 1, TotalLocations = 1, IncompleteLocations = [] },
                AchievementRole.EggMaster => new EggMasterProgress { IsComplete = true },
                AchievementRole.Fabled => new FabledProgress { IsComplete = true, CurrentRank = Rank.Fabled },
                _ => new PowerTypeMasterProgress { IsComplete = true, PowerType = PowerType.Physical, MiceWithMastery = 1, TotalMice = 1, MissingMice = [] }
            };
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
    private async Task<CheckmarkProgress> HasCheckmarkAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserProfileItems(mhId, cancellationToken);

        var completed = (uint)data.Categories.Count(c => c.IsComplete ?? false);
        var total = (uint)data.Categories.Length;
        var incompleteCategories = data.Categories
            .Where(c => !(c.IsComplete ?? false))
            .Select(c => c.Name)
            .ToList();

        return new CheckmarkProgress
        {
            IsComplete = completed == total,
            CompletedCategories = completed,
            TotalCategories = total,
            IncompleteCategories = incompleteCategories
        };
    }

    // Egg Master Achievement: Collect all SEH eggs
    private async Task<EggMasterProgress> HasEggMasterAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var isComplete = await mouseHuntClient.IsEggMaster(mhId, cancellationToken);
        return new EggMasterProgress
        {
            IsComplete = isComplete,
        };
    }

    // If you've got all 10+ catches on all mice (excluding Leppy and Mobster)
    private async Task<CrownProgress> HasCrownAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserMiceAsync(mhId, cancellationToken);
        var mouseNames = await GetMouseNames();

        var relevantMice = data.Mice
            // Filter out Leppy and Mobster
            .Where(m => m.MouseId != 113 && m.MouseId != 128)
            .ToList();

        var miceWithCrown = (uint)relevantMice.Count(m => m.NumCatches >= 10);
        var totalMice = (uint)relevantMice.Count;
        var missingMice = relevantMice
            .Where(m => m.NumCatches < 10)
            .OrderByDescending(m => m.NumCatches)
            .ToDictionary(m => mouseNames.GetValueOrDefault(m.MouseId, $"Mouse #{m.MouseId}"), m => m.NumCatches);

        return new CrownProgress
        {
            IsComplete = miceWithCrown == totalMice,
            MiceWithCrown = miceWithCrown,
            TotalMice = totalMice,
            MissingMice = missingMice
        };
    }

    // If you've got three stars in all locations (meaning you've caught all mice available while in that location)
    private async Task<StarProgress> HasStarAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserLocationStatsAsync(mhId, cancellationToken);

        var completed = (uint)data.Categories.Count(c => c.IsComplete ?? false);
        var total = (uint)data.Categories.Length;
        var incompleteLocations = data.Categories
            .Where(c => !(c.IsComplete ?? false))
            .Select(c => c.Name)
            .ToList();

        return new StarProgress
        {
            IsComplete = completed == total,
            CompletedLocations = completed,
            TotalLocations = total,
            IncompleteLocations = incompleteLocations
        };
    }

    private async Task<FabledProgress> HasFabledRank(uint mhId, CancellationToken cancellationToken = default)
    {
        var titles = await mouseHuntClient.GetTitlesAsync(cancellationToken)
            ?? throw new Exception("Failed to fetch titles from MouseHunt API");

        var userTitle = await mouseHuntClient.GetUserTitleAsync(mhId, cancellationToken)
            ?? throw new Exception("Failed to fetch user title from MouseHunt API");

        var currentRankName = titles.Single(t => t.TitleId == userTitle.TitleId).Name;
        var isComplete = currentRankName == Rank.Fabled;

        return new FabledProgress
        {
            IsComplete = isComplete,
            CurrentRank = currentRankName,
        };
    }

    // Every mouse of given powertype has 100 catches
    private async Task<PowerTypeMasterProgress> HasPowerTypeMastery(uint mhId, PowerType powerType, CancellationToken cancellationToken = default)
    {
        // Get all mice classifications and user's catch data
        var miceById = await GetMiceClassifications();
        var userMice = await mouseHuntClient.GetUserMiceAsync(mhId, cancellationToken);
        var userCatchesByMouseId = userMice.Mice.ToDictionary(m => m.MouseId, m => m.NumCatches);

        // Filter mice by power type and build progress tracking
        var requiredMice = miceById
            .Where(kvp => kvp.Value.powerType == powerType)
            .Select(kvp => new
            {
                MouseId = kvp.Key,  
                Name = kvp.Value.name,
                UserCatches = userCatchesByMouseId.GetValueOrDefault(kvp.Key, 0u)
            })
            .ToList();

        // Separate completed from incomplete mice
        var completedMice = requiredMice.Where(m => m.UserCatches >= 100).ToList();
        var incompleteMice = requiredMice
            .Where(m => m.UserCatches < 100)
            .OrderByDescending(m => m.UserCatches) // Closest to completion first
            .ToDictionary(m => m.Name, m => m.UserCatches);

        return new PowerTypeMasterProgress
        {
            IsComplete = incompleteMice.Count == 0,
            PowerType = powerType,
            MiceWithMastery = (uint)completedMice.Count,
            TotalMice = (uint)requiredMice.Count,
            MissingMice = incompleteMice
        };
    }

    private async Task<Dictionary<uint, string>> GetMouseNames()
    {
        var data = await mouseRipService.GetAllMiceAsync() ?? throw new InvalidOperationException("Failed to retrieve mice from api.mouse.rip.");
        return data.ToDictionary(m => m.Id, m => m.Name);
    }

    private async Task<Dictionary<uint, (string name, PowerType powerType)>> GetMiceClassifications()
    {
        var data = await mouseRipService.GetAllMiceAsync() ?? throw new InvalidOperationException("Failed to retrieve mice from api.mouse.rip.");

        var mice = new Dictionary<uint, (string name, PowerType powerType)>();
        foreach (var mouse in data)
        {
            // Ignore Leppy, Mobster and Event Mice
            if (mouse.Id == 113 || mouse.Id == 128 || mouse.Group == "Event Mice")
            {
                mice[mouse.Id] = (mouse.Name, PowerType.None);
                continue;
            }

            if (mouse.Effectivenesses is null)
            {
                continue;
            }

            mouse.Effectivenesses.Remove(MouseRipEffectiveness.Power);

            var maxEff = mouse.Effectivenesses.Values.Max();
            var keysWithHighestValue = mouse.Effectivenesses.Keys
                .Where(k => mouse.Effectivenesses[k] == maxEff)
                .ToHashSet();

            if (keysWithHighestValue.Count > 1)
            {
                mice[mouse.Id] = (mouse.Name, PowerType.Multi);
            }
            else
            {
                mice[mouse.Id] = (mouse.Name, keysWithHighestValue.Single() switch
                {
                    MouseRipEffectiveness.Arcane => PowerType.Arcane,
                    MouseRipEffectiveness.Draconic => PowerType.Draconic,
                    MouseRipEffectiveness.Forgotten => PowerType.Forgotten,
                    MouseRipEffectiveness.Hydro => PowerType.Hydro,
                    MouseRipEffectiveness.Law => PowerType.Law,
                    MouseRipEffectiveness.Physical => PowerType.Physical,
                    MouseRipEffectiveness.Rift => PowerType.Rift,
                    MouseRipEffectiveness.Shadow => PowerType.Shadow,
                    MouseRipEffectiveness.Tactical => PowerType.Tactical,
                    _ => throw new ArgumentOutOfRangeException($"Unknown effectiveness: {keysWithHighestValue.Single()} for mouse {mouse.Id} ({mouse.Name})"),
                });
            }
        }

        return mice;
    }
}
