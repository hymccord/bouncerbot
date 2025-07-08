using MonstroBot.Rest;

namespace MonstroBot.Services.MouseHunt;
internal class UserAchievementService(MouseHuntRestClient mouseHuntClient)
{
    // Bronze Crown Achievement: Catch 10 of every mouse except Leppy and Mobster
    public async Task<bool> HasBronzedCrownAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserMouseCatches(mhId, cancellationToken);

        return data.Mice
            // Filter out Leppy and Mobster
            .Where(m => m.MouseId != 113 && m.MouseId != 128)
            .All(m => m.NumCatches >= 10);
    }

    public async Task<bool> HasCheckmarkAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var data = await mouseHuntClient.GetUserProfileItems(mhId, cancellationToken);

        return data.Categories.All(c => c.IsComplete);
    }
}
