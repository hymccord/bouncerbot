using BouncerBot.Rest;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;

namespace BouncerBot.Modules.Puzzle;

public interface IPuzzleService
{
    Task TriggerPuzzle();
    Task<bool> SolvePuzzleAsync(string code);
}

internal class PuzzleService(
    IOptionsMonitor<BouncerBotOptions> options,
    RestClient client,
    IMouseHuntRestClient mouseHuntRestClient
    ) : IPuzzleService
{
    private bool _hasPuzzle = false;
    private uint? _hunterId = null;

    public async Task TriggerPuzzle()
    {
        if (_hasPuzzle)
        {
            return;
        }

        _hasPuzzle = true;
        _hunterId ??= (await mouseHuntRestClient.GetMeAsync()).UserId;

        var channel = await client.GetChannelAsync(options.CurrentValue.PuzzleChannel);
        await ((TextChannel)channel).SendMessageAsync(new MessageProperties
        {
            Embeds = [
                new EmbedProperties() {
                    Title = "BouncerBot King's Reward",
                    Description = "One of our guests triggered a puzzle. I need human assistance!",
                    Image = $"https://www.mousehuntgame.com/images/puzzleimage.php?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&user_id={_hunterId}"
                }
            ],
            Components = [
                new ActionRowProperties()
                    .AddButtons(
                        new ButtonProperties("puzzle start", "Solve", ButtonStyle.Success)
                    )
                ]
        });
    }

    public async Task<bool> SolvePuzzleAsync(string code)
    {
        if (!_hasPuzzle)
        {
            return true;
        }

        try
        {
            var success = await mouseHuntRestClient.SolvePuzzleAsync(code);
            if (success)
            {
                _hasPuzzle = false;
            }

            return success;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
