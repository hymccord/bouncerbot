using BouncerBot.Modules.Puzzle;
using BouncerBot.Rest;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
internal class EphemeralApplicationCommandResultHandler<TContext>(
    IOptions<Options> options
    ) : IApplicationCommandResultHandler<TContext>
    where TContext : IApplicationCommandContext
{
    public ValueTask HandleResultAsync(IExecutionResult result, TContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
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
            resultMessage = """
                Apologies, I'm a little busy trying to solve this King's Reward! It's difficult for me... 🤖
                
                Please try again later."
                """;

            services.GetRequiredService<IPuzzleService>().TriggerPuzzle();
        }

        var message = new InteractionMessageProperties()
        {
            Components = new ComponentContainerProperties
            {
                AccentColor = new Color(options.Value.Colors.Error),
                Components = [
                    new TextDisplayProperties(resultMessage)
                    ]
            },
            Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2
        };

        if (failResult is PreconditionFailResult)
        {
            return new(interaction.SendResponseAsync(InteractionCallback.Message(message)));
        }

        return new(interaction.SendFollowupMessageAsync(message));
    }
}

internal class EphemeralComponentInteractionResultHandler<TContext>(
    IOptions<Options> options
    ) : IComponentInteractionResultHandler<TContext>
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
            resultMessage = """
                Apologies, I'm a little busy trying to solve this King's Reward! It's difficult for me... 🤖
                
                Please try again later."
                """;

            services.GetRequiredService<IPuzzleService>().TriggerPuzzle();
        }

        var message = new InteractionMessageProperties()
        { 
            Components = new ComponentContainerProperties
            {
                AccentColor = new Color(options.Value.Colors.Error),
                Components = [
                    new TextDisplayProperties(resultMessage)
                    ]
            },
            Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2
        };

        if (failResult is PreconditionFailResult)
        {
            return new(interaction.SendResponseAsync(InteractionCallback.Message(message)));
        }

        return new(interaction.SendFollowupMessageAsync(message));
    }
}
