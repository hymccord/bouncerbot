using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

namespace MonstroBot;
internal class EphemeralApplicationCommandResultHandler : IApplicationCommandResultHandler<ApplicationCommandContext>
{
    private static readonly ApplicationCommandResultHandler<ApplicationCommandContext> _defaultHandler = new(MessageFlags.Ephemeral);

    public ValueTask HandleResultAsync(IExecutionResult result, ApplicationCommandContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
    {
        logger.LogInformation("Handling result of an application command");

        return _defaultHandler.HandleResultAsync(result, context, client, logger, services);
    }
}

internal class EphemeralComponentInteractionResultHandler : IComponentInteractionResultHandler<ComponentInteractionContext>
{
    private static readonly ComponentInteractionResultHandler<ComponentInteractionContext> _defautHandler = new ComponentInteractionResultHandler<ComponentInteractionContext>(MessageFlags.Ephemeral);

    public ValueTask HandleResultAsync(IExecutionResult result, ComponentInteractionContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
    {
        logger.LogInformation("Handling result of an component interaction");

        return _defautHandler.HandleResultAsync(result, context, client, logger, services);
    }
}
