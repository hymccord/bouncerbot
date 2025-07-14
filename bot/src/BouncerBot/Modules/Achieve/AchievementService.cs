using BouncerBot.Rest;

namespace BouncerBot.Modules.Achieve;
internal class AchievementService(MouseHuntRestClient mouseHuntClient)
{
    // Checkmark Achievement: Collect all non LE items
    public async Task<bool> HasCheckmarkAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserProfileItems(mhId, cancellationToken);

        return data.Categories.All(c => c.IsComplete);
    }

    public async Task<bool> HasEggMasterAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserMouseCatches(mhId, cancellationToken);

        return false;
    }

    // Bronze Crown Achievement: Catch 10 of every mouse except Leppy and Mobster
    public async Task<bool> HasCrownAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserMouseCatches(mhId, cancellationToken);

        return data.Mice
            // Filter out Leppy and Mobster
            .Where(m => m.MouseId != 113 && m.MouseId != 128)
            .All(m => m.NumCatches >= 10);
    }

    public async Task<bool> HasStarAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserLocationStatsAsync(mhId, cancellationToken);

        return data.Categories.All(c => c.IsComplete);
    }
}
