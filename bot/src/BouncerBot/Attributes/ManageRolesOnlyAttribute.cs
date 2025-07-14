using NetCord.Services;

namespace BouncerBot.Attributes;

internal sealed class ManageRolesOnlyAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public ManageRolesOnlyAttribute() : base(NetCord.Permissions.ManageRoles)
    {
    }
}
