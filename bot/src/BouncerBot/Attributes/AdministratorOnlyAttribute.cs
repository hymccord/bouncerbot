using NetCord.Services;

namespace BouncerBot.Attributes;

internal sealed class AdministratorOnlyAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public AdministratorOnlyAttribute() : base(default, NetCord.Permissions.Administrator)
    {
    }
}
