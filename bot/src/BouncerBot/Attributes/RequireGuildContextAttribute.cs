using NetCord.Services;

namespace BouncerBot.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
sealed class RequireGuildContextAttribute<TContext> : RequireContextAttribute<TContext>
    where TContext : IGuildContext
{
    public RequireGuildContextAttribute() : base(RequiredContext.Guild, "This action can only be used in a server.")
    {
    }
}
