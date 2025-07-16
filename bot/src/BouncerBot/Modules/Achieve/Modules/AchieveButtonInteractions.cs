using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Achieve.Modules;

public class AchieveButtonInteractions(AchievementRoleOrchestrator achievementRoleOrchestrator) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("achieve reset confirm")]
    public async Task ResetConfirmAsync(AchievementRole achievement)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await ModifyResponseAsync(m =>
        {
            m.Content = "Preparing...";
            m.Components = [];
        });

        try
        {
            await achievementRoleOrchestrator.ResetAchievementsAsync(Context.Guild!.Id, achievement, async (current, total) =>
            {
                await ModifyResponseAsync(m =>
                    m.Content = $"""
                        Resetting achievement... {current}/{total} users processed.
                        Please wait, this may take a while.
                        """
                );
            });

            await ModifyResponseAsync(m =>
            {
                m.Content = "Achievement reset completed successfully!";
            });
        }
        catch (Exception ex)
        {
            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                    An error occurred while resetting achievements. Please try again later.
                    Error: `{ex.Message}`
                    """;
            });
        }
    }

    [ComponentInteraction("achieve reset cancel")]
    public async Task ResetCancelAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }
}
