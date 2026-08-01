using BouncerBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace BouncerBot.GatewayHandlers;
public class GuildCreateGatewayHandler(
    ILogger<GuildCreateGatewayHandler> logger,
    GatewayClient gatewayClient
    ) : IGuildCreateGatewayHandler
{
    public async ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        var guild = arg.Guild;
        if (guild is null)
        {
            return;
        }

        logger.LogInformation("""
            Requesting all users (Qty: {UserCount}) in {GuildName} ({GuildId}). IsLarge: {IsLarge}.

            {CachedUsers} users are cached.
            """, guild.UserCount, guild.Name, guild.Id, guild.Users.Count, guild.IsLarge);

        await gatewayClient.RequestGuildUsersAsync(new GuildUsersRequestProperties(arg.GuildId)
            .WithQuery(""));
    }
}
