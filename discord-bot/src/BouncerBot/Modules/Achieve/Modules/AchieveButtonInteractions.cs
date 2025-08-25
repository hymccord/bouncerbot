using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Achieve.Modules;

public class AchieveButtonInteractions(
    IOptions<BouncerBotOptions> options,
    IAchievementRoleOrchestrator achievementRoleOrchestrator)
    : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("achieve verify share")]
    public async Task ShareAchievementAsync(string content)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await DeleteResponseAsync();

        _ = Context.Channel.SendMessageAsync(new MessageProperties
        {
            Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Primary))
                    .AddComponents(
                        new TextDisplayProperties("**Achievement Status**"),
                        new ComponentSeparatorProperties().WithSpacing(ComponentSeparatorSpacingSize.Small).WithDivider(true),
                        new TextDisplayProperties(content)
                    )
            ],
            Flags = MessageFlags.IsComponentsV2
        });
    }

    [ComponentInteraction("achieve reset confirm")]
    public async Task ResetConfirmAsync(AchievementRole achievement)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await ModifyResponseAsync(m =>
        {
            m.Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Primary))
                    .AddComponents(
                        new TextDisplayProperties("Preparing...")
                    )
                ];
            m.Flags = MessageFlags.IsComponentsV2;
        });

        try
        {
            await achievementRoleOrchestrator.ResetAchievementAsync(Context.Guild!.Id, achievement, async (current, total) =>
            {
                await ModifyResponseAsync(m =>
                {
                    m.Components = [
                        new ComponentContainerProperties()
                            .WithAccentColor(new Color(options.Value.Colors.Primary))
                            .AddComponents(
                                new TextDisplayProperties($"""
                                    Resetting achievement... {current}/{total} users processed.
                                    Please wait, this may take a while.
                                    """)
                            )
                        ];
                    m.Flags = MessageFlags.IsComponentsV2;
                }
                );
            });

            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Success))
                    .AddComponents(
                        new TextDisplayProperties("Achievement reset completed successfully!")
                    )
                ];
                m.Flags = MessageFlags.IsComponentsV2;
            });
        }
        catch (Exception ex)
        {
            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Error))
                    .AddComponents(
                        new TextDisplayProperties($"""
                            An error occurred while resetting achievements. Please try again later.
                            Error: `{ex.Message}`
                            """)
                    )
                ];
                m.Flags = MessageFlags.IsComponentsV2;
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
