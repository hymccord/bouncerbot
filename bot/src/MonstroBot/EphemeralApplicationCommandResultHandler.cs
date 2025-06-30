using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ApplicationCommands;
using NetCord.Services;
using NetCord;
using Microsoft.Extensions.Logging;

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
