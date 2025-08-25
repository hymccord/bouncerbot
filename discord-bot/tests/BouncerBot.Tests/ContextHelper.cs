using System.Reflection;

using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Tests;

internal static class HelperExtensions
{
    extension<TContext>(BaseApplicationCommandModule<TContext> commandModule)
        where TContext : IApplicationCommandContext
    {
        public void SetContext(TContext context)
        {
            var method = typeof(BaseApplicationCommandModule<TContext>)
                .GetMethod("SetContext", BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.MakeGenericMethod(typeof(TContext))
                .Invoke(commandModule, [context]);
        }
    }
}
