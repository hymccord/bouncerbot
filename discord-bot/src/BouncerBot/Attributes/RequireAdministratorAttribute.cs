using NetCord.Services;

namespace BouncerBot.Attributes;

internal sealed class RequireAdministratorAttribute<TContext> : RequireUserPermissionsAttribute<TContext>
    where TContext : IUserContext, IGuildContext, IChannelContext
{
    public RequireAdministratorAttribute() : base(default, NetCord.Permissions.Administrator)
    {
    }
}
