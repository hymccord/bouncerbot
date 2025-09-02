using BouncerBot.Rest;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Puzzle;
public class PuzzleButtonInteraction : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("puzzle start")]
    public static InteractionCallbackProperties StartPuzzleModal()
    {
        return InteractionCallback.Modal(new ModalProperties("puzzle-modal", "BouncerBot's King's Reward")
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
    public async Task SolvePuzzleAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var code = Context.Components.OfType<TextInput>().FirstOrDefault(c => c.CustomId == "puzzle-code")?.Value ?? string.Empty;

        await ModifyResponseAsync(m =>
        {
            m.Components = [];
        });

        var success = await puzzleService.SolvePuzzleAsync(code);

        if (success)
        {
            await DeleteResponseAsync();
        }
        else
        {
            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ActionRowProperties()
                        .AddComponents([
                            new ButtonProperties("puzzle start", "Try Again", ButtonStyle.Success)
                            ])
                ];
            });
        }
    }
}
