using BouncerBot.Rest;
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
    IMouseHuntRestClient mouseHuntRestClient
    ) : IPuzzleService
{
    private bool _hasPuzzle = false;
    private ulong? _puzzleMessageId = null;
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
        var message = await ((TextChannel)channel).SendMessageAsync(new MessageProperties
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
                    .AddComponents(
                        new ButtonProperties("puzzle start", "Solve", ButtonStyle.Success)
                    )
                ]
        });

        _puzzleMessageId = message.Id;
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

            if (_puzzleMessageId.HasValue)
            {
                await client.DeleteMessageAsync(options.CurrentValue.PuzzleChannel, _puzzleMessageId.Value);
                _puzzleMessageId = null;
            }

            if (!success)
            {
                logger.LogInformation("Puzzle attempt was incorrect.");
            }

            return success;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _hasPuzzle = false;
        }
    }
}
