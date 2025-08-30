using System.Diagnostics;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using BouncerBot.Rest;
using Grafana.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using OpenTelemetry.Resources;

namespace BouncerBot;

#pragma warning disable IDE0079
#pragma warning disable CA1822

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
        public IHostApplicationBuilder ConfigureMetrics()
        {
            builder.ConfigureOpenTelemetry();
            builder.ConfigureSentry();

            return builder;
        }

        public IHostApplicationBuilder ConfigureOpenTelemetry()
        {
            // Build a resource configuration action to set service information.
            void ConfigureResource(ResourceBuilder r) => r.AddService(
                serviceName: builder.Configuration.GetValue("ServiceName", defaultValue: "BouncerBot")!,
                serviceVersion: typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion ?? "unknown",
                serviceInstanceId: Environment.MachineName);

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(ConfigureResource)
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
                builder.Services.AddOpenTelemetry()
                    .UseAzureMonitorExporter();
            }

            var exporter = builder.Configuration.GetSection("Grafana").Get<OtlpExporter>();
            if (exporter is not null)
            {
                builder.Services.AddOpenTelemetry()
                    .UseGrafana(config =>
                    {
                        config.ExporterSettings = exporter;
                    });
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
