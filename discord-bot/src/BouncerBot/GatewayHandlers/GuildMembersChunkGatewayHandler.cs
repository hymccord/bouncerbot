using BouncerBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace BouncerBot.GatewayHandlers;

public class GuildMembersChunkGatewayHandler(
    ILogger<GuildMembersChunkGatewayHandler> logger,
    IServiceScopeFactory serviceScopeFactory
    ) : IGuildUserChunkGatewayHandler
{
    public ValueTask HandleAsync(GuildUserChunkEventArgs arg)
    {
        logger.LogInformation("Received GUILD_MEMBERS_CHUNK for {GuildId}. Received {ReceivedCount} users. Chunk index: {ChunkIndex}, Chunk count: {ChunkCount}.",
            arg.GuildId, arg.Users.Count, arg.ChunkIndex, arg.ChunkCount);

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var guildUserRoleMonitor = scope.ServiceProvider.GetRequiredService<IGuildRoleMembershipSynchronizer>();

            return new ValueTask(guildUserRoleMonitor.ProcessCachedUsersAsync(arg.GuildId, arg.Users.ToDictionary(u => u.Id)));
        }
    }
}
