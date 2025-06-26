using NetCord.Services;

namespace MonstroBot.Attributes;

internal sealed class AdministratorOnlyAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public AdministratorOnlyAttribute() : base(default, NetCord.Permissions.Administrator)
    {
    }
}
