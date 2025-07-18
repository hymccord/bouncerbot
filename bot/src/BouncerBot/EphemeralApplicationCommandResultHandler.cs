using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot;
internal class EphemeralApplicationCommandResultHandler : IApplicationCommandResultHandler<ApplicationCommandContext>
{
    private static readonly ApplicationCommandResultHandler<ApplicationCommandContext> _defaultHandler = new(MessageFlags.Ephemeral);

    public ValueTask HandleResultAsync(IExecutionResult result, ApplicationCommandContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
    {
        return _defaultHandler.HandleResultAsync(result, context, client, logger, services);
    }
}

internal class EphemeralComponentInteractionResultHandler<TContext> : IComponentInteractionResultHandler<TContext>
    where TContext : IComponentInteractionContext
{
    private static readonly ComponentInteractionResultHandler<TContext> _defautHandler = new ComponentInteractionResultHandler<TContext>(MessageFlags.Ephemeral);

    public ValueTask HandleResultAsync(IExecutionResult result, TContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
    {
        return _defautHandler.HandleResultAsync(result, context, client, logger, services);
    }
}
