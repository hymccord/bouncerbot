using Azure.Monitor.OpenTelemetry.Exporter;
using BouncerBot.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

#pragma warning disable CA1822

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

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder ConfigureOpenTelemetry()
        {
            builder.Services.AddOpenTelemetry()
                .WithMetrics(b =>
                {
                    b.AddMeter("BouncerBot");
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        public IHostApplicationBuilder AddOpenTelemetryExporters()
        {
            var useAzureMonitor = !string.IsNullOrEmpty(builder.Configuration["AzureMonitorExporter:ConnectionString"]);
            if (useAzureMonitor)
            {
                builder.Services.AddOpenTelemetry().UseAzureMonitorExporter();
            }
            
            return builder;
        }

        public IHostApplicationBuilder ConfigureSentry()
        {
            var useSentry = !string.IsNullOrEmpty(builder.Configuration["Sentry:Dsn"]);

            if (useSentry)
            {
                builder.Logging.AddConfiguration(builder.Configuration);
                builder.Logging.AddSentry();
            }

            return builder;
        }
    }
}

#pragma warning restore
