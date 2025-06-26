using NetCord.Services;

namespace MonstroBot.Attributes;

internal sealed class ManageMessageOnlyAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public ManageMessageOnlyAttribute() : base(NetCord.Permissions.ManageMessages)
    {
    }
}
