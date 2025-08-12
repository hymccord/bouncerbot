using BouncerBot.Rest;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot;
public static class Extensions
{
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMouseHuntClient()
        {
            services
                .AddOptions<MouseHuntRestClientOptions>()
                .BindConfiguration("MouseHunt");

            services.AddSingleton<IMouseHuntRestClient, MouseHuntRestClient>();

            services.AddHostedService<MouseHuntRestClientHostedService>();

            return services;
        }

        public IServiceCollection AddComponentInteractionWithEphemeralResultHandler<TInteraction, TContext>()
            where TInteraction : ComponentInteraction
            where TContext : IComponentInteractionContext
        {
            services.AddComponentInteractions<TInteraction, TContext>((options, services) =>
            {
                options.ResultHandler = new EphemeralComponentInteractionResultHandler<TContext>(services.GetRequiredService<IOptions<Options>>());
            });

            return services;
        }
    }
#pragma warning restore CA1822,IDE0079

}
