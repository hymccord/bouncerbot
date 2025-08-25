using NetCord.Services;

namespace BouncerBot.Attributes;

internal sealed class RequireOwnerAttribute<TContext> : PreconditionAttribute<TContext>
    where TContext : IUserContext, IGatewayClientContext
{
    public override async ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        var application = await context.Client.Rest.GetCurrentApplicationAsync();
        var userId = context.User.Id;

        if (application.Owner.Id == userId)
        {
            return PreconditionResult.Success;
        }

        return PreconditionResult.Fail("You don't have permission to use this command.");
    }
}
