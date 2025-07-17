using NetCord.Services;

namespace BouncerBot.Attributes;

internal sealed class RequireManageRolesAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public RequireManageRolesAttribute() : base(NetCord.Permissions.ManageRoles)
    {
    }
}
