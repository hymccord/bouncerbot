using BouncerBot.Rest;

using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMouseHuntClient(this IServiceCollection services)
    {
        services
            .AddOptions<MouseHuntRestClientOptions>()
            .BindConfiguration("MouseHunt");

        services.AddSingleton<MouseHuntRestClient>();

        services.AddHostedService<MouseHuntRestClientHostedService>();

        return services;
    }

    public static IServiceCollection AddComponentInteractionWithEphemeralResultHandler<TInteraction, TContext>(this IServiceCollection services)
        where TInteraction : ComponentInteraction
        where TContext : IComponentInteractionContext
    {
        services.AddComponentInteractions<TInteraction, TContext>(options =>
        {
            options.ResultHandler = new EphemeralComponentInteractionResultHandler<TContext>();
        });

        return services;
    }
}
