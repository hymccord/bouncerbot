using Microsoft.Extensions.DependencyInjection;

using MonstroBot.Rest;

namespace MonstroBot;
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
}
