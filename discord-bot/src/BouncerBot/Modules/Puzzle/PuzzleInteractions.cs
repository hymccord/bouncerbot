using BouncerBot.Rest;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Puzzle;
public class PuzzleButtonInteraction : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("puzzle start")]
    public static InteractionCallbackProperties StartPuzzleModal(uint hunterId)
    {
        return InteractionCallback.Modal(new ModalProperties($"puzzle-modal:{hunterId}", "BouncerBot's King's Reward")
            .AddComponents(
                new LabelProperties("Puzzle",
                    new TextInputProperties("puzzle-code", TextInputStyle.Short)
                        .WithPlaceholder("Enter the code here")
                        .WithRequired(true)
                        .WithMinLength(5)
                        .WithMaxLength(5)
            )));
    }
}

public class PuzzleModalInteraction(
    IPuzzleService puzzleService
    ) : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction("puzzle-modal")]
    public async Task SolvePuzzleAsync(uint hunterId)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var code = ((TextInput)((Label)(Context.Components[0])).Component).Value;

        await ModifyResponseAsync(m =>
        {
            m.Content = "Submitting response...";
            m.Embeds = [];
            m.Components = [];
        });

        var success = await puzzleService.SolvePuzzleAsync(code);

        if (success)
        {
            await DeleteResponseAsync();
        }
        else
        {
            var message = PuzzleService.BuildPuzzleMessage(hunterId);

            await ModifyResponseAsync(m =>
            {
                m.Content = "That code was not correct or something went wrong. Please try again.";
                m.Embeds = message.Embeds;
                m.Components = [
                    new ActionRowProperties()
                        .AddComponents(
                        new ButtonProperties($"puzzle start:{hunterId}", "Try Again", ButtonStyle.Success)
                        )
                ];
            });
        }
    }
}
