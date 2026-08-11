using BouncerBot.Modules.ColorRole;
using NetCord;
using NetCord.Gateway;
using DiscordRole = NetCord.Role;
using NetCord.Hosting.Gateway;

namespace BouncerBot.GatewayHandlers;

public sealed class ColorRoleGatewayHandler(IColorRoleRegistry colorRoleRegistry) :
    IGuildCreateGatewayHandler,
    IGuildUpdateGatewayHandler,
    IRoleCreateGatewayHandler,
    IRoleUpdateGatewayHandler,
    IRoleDeleteGatewayHandler
{
    public ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        if (arg.Guild is not null)
        {
            colorRoleRegistry.UpdateGuild(arg.Guild);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask HandleAsync(Guild guild)
    {
        colorRoleRegistry.UpdateGuild(guild);
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleAsync(DiscordRole role)
    {
        colorRoleRegistry.UpdateRole(role);
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleAsync(RoleDeleteEventArgs role)
    {
        colorRoleRegistry.RemoveRole(role);
        return ValueTask.CompletedTask;
    }
}
