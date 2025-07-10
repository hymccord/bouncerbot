using NetCord.Services;

namespace BouncerBot.Attributes;

internal sealed class ManageMessageOnlyAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public ManageMessageOnlyAttribute() : base(NetCord.Permissions.ManageMessages)
    {
    }
}
