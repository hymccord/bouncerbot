using BouncerBot.Rest;
using BouncerBot.Services;
using Microsoft.Extensions.Logging;
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
    ILogger<PuzzleService> logger,
    RestClient client,
    IMouseHuntRestClient mouseHuntRestClient,
    IBouncerBotPresenceService presenceService
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

        await presenceService.SetPuzzlePresence();

        _hasPuzzle = true;
        _hunterId ??= (await mouseHuntRestClient.GetMeAsync()).UserId;

        var channel = await client.GetChannelAsync(options.CurrentValue.PuzzleChannel);
        await ((TextChannel)channel).SendMessageAsync(BuildPuzzleMessage(_hunterId.Value));
    }

    public async Task<bool> SolvePuzzleAsync(string code)
    {
        if (!_hasPuzzle)
        {
            return true;
        }

        var success = false;

        try
        {
            success = await mouseHuntRestClient.SolvePuzzleAsync(code);

            _hasPuzzle = !success;
            if (!_hasPuzzle)
            {
                await presenceService.SetDefaultPresence();
            }
        }
        catch (Exception)
        {
            logger.LogInformation("Puzzle attempt failed.");
        }

        return success;
    }

    public static MessageProperties BuildPuzzleMessage(uint hunterId)
    {
        return new MessageProperties
        {
            Embeds = [
                new EmbedProperties() {
                    Title = "BouncerBot King's Reward",
                    Description = "One of our guests triggered a puzzle. I need human assistance!",
                    Image = $"https://www.mousehuntgame.com/images/puzzleimage.php?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&user_id={hunterId}"
                }
            ],
            Components = [
                new ActionRowProperties()
                    .AddComponents(
                        new ButtonProperties($"puzzle start:{hunterId}", "Solve", ButtonStyle.Success)
                    )
                ]
        };
    }
}
