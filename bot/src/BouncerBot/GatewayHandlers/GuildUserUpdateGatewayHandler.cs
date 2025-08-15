using BouncerBot.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace BouncerBot.GatewayHandlers;

public class GuildUserUpdateGatewayHandler(ILogger<GuildUserUpdateGatewayHandler> logger, GatewayClient client, IServiceScopeFactory scopeFactory) : IGuildUserUpdateGatewayHandler
{
    public async ValueTask HandleAsync(GuildUser guildUser)
    {
        // Since this class is a singleton, we need to create a scope for possible transient services
        using (var scope = scopeFactory.CreateScope())
        {
            // If we can get the user role cache, we can fire role added or role removed

            if (client.Cache.Guilds.TryGetValue(guildUser.GuildId, out var guild)
                && guild.Users.TryGetValue(guildUser.Id, out var oldUser))
            {
                var oldRoles = oldUser.RoleIds.ToHashSet();
                var newRoles = guildUser.RoleIds.ToHashSet();

                if (oldRoles.Count != newRoles.Count)
                {
                    var monitor = scope.ServiceProvider.GetRequiredService<IGuildUserRoleMonitorService>();

                    await monitor.HandleRolesRemovedAsync(guildUser, oldRoles.Except(newRoles));
                    await monitor.HandleRolesAddedAsync(guildUser, newRoles.Except(oldRoles));
                }
            }
        }
    }
}
