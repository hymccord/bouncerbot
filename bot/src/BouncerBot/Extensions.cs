using BouncerBot.Rest;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot;
public static class Extensions
{
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
                options.ResultHandler = new EphemeralComponentInteractionResultHandler<TContext>(services.GetRequiredService<IOptionsMonitor<BouncerBotOptions>>());
            });

            return services;
        }
    }
}
