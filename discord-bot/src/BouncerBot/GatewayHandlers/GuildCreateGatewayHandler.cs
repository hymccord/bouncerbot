using BouncerBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace BouncerBot.GatewayHandlers;
public class GuildCreateGatewayHandler(
    ILogger<GuildCreateGatewayHandler> logger,
    GatewayClient gatewayClient,
    IServiceScopeFactory serviceScopeFactory
    ) : IGuildCreateGatewayHandler
{
    public async ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        var guild = arg.Guild;
        if (guild is null)
        {
            return;
        }

        if (guild.IsLarge)
        {
            logger.LogInformation("""
                Encountered GUILD_CREATE for large guild. Requesting all users (Qty: {UserCount}) in {GuildName} ({GuildId}).

                {CachedUsers} users are cached.
                """, guild.UserCount, guild.Name, guild.Id, guild.Users.Count);

            await gatewayClient.RequestGuildUsersAsync(new GuildUsersRequestProperties(arg.GuildId));
        }

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var guildUserRoleMonitor = scope.ServiceProvider.GetRequiredService<IGuildRoleMembershipSynchronizer>();
            await guildUserRoleMonitor.ProcessCachedUsersAsync(arg.GuildId, guild.Users);
        }
    }
}
