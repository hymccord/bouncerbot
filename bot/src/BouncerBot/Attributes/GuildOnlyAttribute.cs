using NetCord.Services;

namespace BouncerBot.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
sealed class GuildOnlyAttribute<TContext> : RequireContextAttribute<TContext>
    where TContext : IGuildContext
{
    public GuildOnlyAttribute() : base(RequiredContext.Guild, "This action can only be used in a server.")
    {
    }
}
