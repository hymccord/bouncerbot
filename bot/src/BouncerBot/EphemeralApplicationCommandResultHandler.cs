using BouncerBot.Modules.Puzzle;
using BouncerBot.Rest;
using BouncerBot.Rest.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

// Summary of two main changes from default handlers:
// 1. MessageFlags.Ephemeral is set, so the response is only visible to the user who invoked the command.
// 2. Use SendFollowupMessageAsync instead of RespondAsync since we defer ephemerally for most of our initial responses.

// Note that if you DeferMessage without the ephemeral flag, followup messages will also not be ephemeral.

namespace BouncerBot;
internal class EphemeralApplicationCommandResultHandler : IApplicationCommandResultHandler<ApplicationCommandContext>
{
    public ValueTask HandleResultAsync(IExecutionResult result, ApplicationCommandContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
    {
        if (result is not IFailResult failResult)
            return default;

        var resultMessage = failResult.Message;

        var interaction = context.Interaction;

        if (failResult is IExceptionResult exceptionResult)
            logger.LogError(exceptionResult.Exception, "Execution of an application command of name '{Name}' failed with an exception", interaction.Data.Name);
        else
            logger.LogDebug("Execution of an application command of name '{Name}' failed with '{Message}'", interaction.Data.Name, resultMessage);

        if (failResult is IExceptionResult { Exception: PuzzleException })
        {
            resultMessage = "Apologies, my account has a King's Reward and needs human intervention. I've notified the appropriate people. Please try again later.";
            var puzzleHelper = services.GetRequiredService<IPuzzleService>();
            _ = puzzleHelper.TriggerPuzzle();
        }

        return new(interaction.SendFollowupMessageAsync(new()
        {
            Components = new ComponentContainerProperties
            {
                AccentColor = new Color(0xFF, 0x00, 0x00),
                Components = [
                    new TextDisplayProperties(resultMessage)
                    ]
            },
            Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2
        }));
    }
}

internal class EphemeralComponentInteractionResultHandler<TContext> : IComponentInteractionResultHandler<TContext>
    where TContext : IComponentInteractionContext
{
    public ValueTask HandleResultAsync(IExecutionResult result, TContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
    {
        if (result is not IFailResult failResult)
            return default;

        var resultMessage = failResult.Message;

        var interaction = context.Interaction;

        if (failResult is IExceptionResult exceptionResult)
            logger.LogError(exceptionResult.Exception, "Execution of an interaction of custom ID '{Id}' failed with an exception", interaction.Data.CustomId);
        else
            logger.LogDebug("Execution of an interaction of custom ID '{Id}' failed with '{Message}'", interaction.Data.CustomId, resultMessage);

        
        if (failResult is IExceptionResult { Exception: PuzzleException })
        {
            resultMessage = "Apologies, my account has a King's Reward and needs human intervention. I've notified the appropriate people. Please try again later.";

            var puzzleHelper = services.GetRequiredService<IPuzzleService>();
            _ = puzzleHelper.TriggerPuzzle();
        }

        return new(interaction.SendFollowupMessageAsync(new()
        {
            Components = new ComponentContainerProperties
            {
                AccentColor = new Color(0xFF, 0x00, 0x00),
                Components = [
                    new TextDisplayProperties(resultMessage)
                    ]
            },
            Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2
        }));
    }
}
