using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace BouncerBot.GatewayHandlers;
public class GuildCreateGatewayHandler(
    ILogger<GuildCreateGatewayHandler> logger,
    GatewayClient gatewayClient
    ) : IGuildCreateGatewayHandler
{
    public ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        var guild = arg.Guild;
        if (guild is null)
        {
            return default;
        }

        if (!guild.IsLarge)
        {
            return default;
        }

        logger.LogInformation("Encountered GUILD_CREATE for large guild. Requesting all users in {GuildName} ({GuildId})", guild.Name, guild.Id);

        return gatewayClient.RequestGuildUsersAsync(new GuildUsersRequestProperties(arg.GuildId));
    }
}
