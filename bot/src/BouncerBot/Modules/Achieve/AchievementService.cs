using BouncerBot.Rest;

namespace BouncerBot.Modules.Achieve;
public class AchievementService(MouseHuntRestClient mouseHuntClient)
{
    public async Task<bool> HasAchievementAsync(uint mhId, AchievementRole achievement, CancellationToken cancellationToken = default)
    {
        return achievement switch
        {
            AchievementRole.Checkmark => await HasCheckmarkAsync(mhId, cancellationToken),
            AchievementRole.EggMaster => await HasEggMasterAsync(mhId, cancellationToken),
            AchievementRole.Crown => await HasCrownAsync(mhId, cancellationToken),
            AchievementRole.Star => await HasStarAsync(mhId, cancellationToken),
            AchievementRole.ArcaneMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Arcane, cancellationToken),
            AchievementRole.DraconicMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Draconic, cancellationToken),
            AchievementRole.ForgottenMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Forgotten, cancellationToken),
            AchievementRole.HydroMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Hydro, cancellationToken),
            AchievementRole.LawMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Law, cancellationToken),
            AchievementRole.PhysicalMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Physical, cancellationToken),
            AchievementRole.RiftMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Rift, cancellationToken),
            AchievementRole.ShadowMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Shadow, cancellationToken),
            AchievementRole.TacticalMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Tactical, cancellationToken),
            AchievementRole.MultiMaster => await HasPowerTypeMastery(mhId, PowerTypeMastery.Multi, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(achievement), achievement, null)
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

    private async Task<bool> HasPowerTypeMastery(uint mhId, PowerTypeMastery powerType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException($"Power type mastery for {powerType} is not implemented yet.");
    }
}
