using System.Reflection;

using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Tests;

internal static class HelperExtensions
{
    extension<TContext>(BaseApplicationCommandModule<TContext> commandModule)
        where TContext : IApplicationCommandContext
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Extension member analyzer bug")]
        public void SetContext(TContext context)
        {
            var method = typeof(BaseApplicationCommandModule<TContext>)
                .GetMethod("SetContext", BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.MakeGenericMethod(typeof(TContext))
                .Invoke(commandModule, [context]);
        }
    }
}
