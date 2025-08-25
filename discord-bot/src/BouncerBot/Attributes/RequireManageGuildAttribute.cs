using NetCord.Services;

namespace BouncerBot.Attributes;

internal sealed class RequireManageGuildAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public RequireManageGuildAttribute() : base(NetCord.Permissions.ManageGuild)
    {
    }
}
