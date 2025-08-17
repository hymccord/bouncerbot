using Azure.Monitor.OpenTelemetry.Exporter;
using BouncerBot;
using BouncerBot.Db;
using BouncerBot.Modules.Achieve;
using BouncerBot.Modules.Bounce;
using BouncerBot.Modules.Config;
using BouncerBot.Modules.Puzzle;
using BouncerBot.Modules.Verification;
using BouncerBot.Modules.WhoIs;
using BouncerBot.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Sentry.Extensions.Logging.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services
    .AddDbContextFactory<BouncerBotDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("BouncerBot"));
    })
    .AddTransient<IGuildLoggingService, GuildLoggingService>()
    .AddTransient<IGuildUserRoleMonitorService, GuildUserRoleMonitor>()
    .AddTransient<IRandomPhraseGenerator, RandomPhraseGenerator>()
    .AddTransient<IAchievementMessageService, AchievementMessageService>()
    .AddTransient<IRoleService, DiscordRoleService>()
    .AddTransient<IAchievementRoleOrchestrator, AchievementRoleOrchestrator>()
    .AddTransient<IAchievementService, AchievementService>()
    .AddTransient<IBounceService, BounceService>()
    .AddTransient<IBounceOrchestrator, BounceOrchestrator>()
    .AddTransient<IConfigService, ConfigService>()
    .AddTransient<IVerificationOrchestrator, VerificationOrchestrator>()
    .AddTransient<IVerificationService, VerificationService>()
    .AddTransient<IWhoIsService, WhoIsService>()
    .AddTransient<IWhoIsOrchestrator, WhoIsOrchestrator>()
    .AddTransient<ICommandMentionService, CommandMentionService>()
    .AddSingleton<IDiscordRestClient, DiscordRestClient>()
    .AddSingleton<IDiscordGatewayClient, DiscordGatewayClient>()
    .AddSingleton<IPuzzleService, PuzzleService>() // Singleton b/c of puzzle state capture
    .AddMouseHuntClient()
    ;

builder.Services
    .AddOptions<BouncerBotOptions>()
    .BindConfiguration(nameof(BouncerBotOptions))
    .ValidateDataAnnotations();

builder.Services
    .AddTransient<IMouseRipService, MouseRipService>()
    .AddHttpClient<MouseRipService>((sp, httpClient) =>
    {
        httpClient.BaseAddress = new Uri("https://api.mouse.rip/");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BouncerBot/1.0 (Discord: Xellis)");
    });

builder.Services.AddOpenTelemetry().UseAzureMonitorExporter();
if (!string.IsNullOrEmpty(builder.Configuration["Sentry:Dsn"]))
{
    builder.Logging.AddConfiguration(builder.Configuration);
    builder.Logging.AddSentry();
}

// NetCord services
builder.Services
    .AddSingleton<IdApplicationCommandServiceStorage<ApplicationCommandContext>>()
    .AddApplicationCommands((options, services) =>
    {
        options.Storage = services.GetRequiredService<IdApplicationCommandServiceStorage<ApplicationCommandContext>>();
        options.ResultHandler = new EphemeralApplicationCommandResultHandler<ApplicationCommandContext>(services.GetRequiredService<IOptionsMonitor<BouncerBotOptions>>());
    })
    // Custom helper extension to easily add ephemeral result handlers for component interactions
    .AddComponentInteractionWithEphemeralResultHandler<ButtonInteraction, ButtonInteractionContext>()
    .AddComponentInteractionWithEphemeralResultHandler<StringMenuInteraction, StringMenuInteractionContext>()
    .AddComponentInteractionWithEphemeralResultHandler<UserMenuInteraction, UserMenuInteractionContext>()
    .AddComponentInteractionWithEphemeralResultHandler<RoleMenuInteraction, RoleMenuInteractionContext>()
    .AddComponentInteractionWithEphemeralResultHandler<MentionableMenuInteraction, MentionableMenuInteractionContext>()
    .AddComponentInteractionWithEphemeralResultHandler<ChannelMenuInteraction, ChannelMenuInteractionContext>()
    .AddComponentInteractionWithEphemeralResultHandler<ModalInteraction, ModalInteractionContext>()
    .AddDiscordGateway((options, services) =>
    {
        options.Presence = new PresenceProperties(UserStatusType.Online)
        {
            Activities = [new("MouseHunters!", UserActivityType.Watching)]
        };
        options.Intents = 0
            | GatewayIntents.Guilds             // For joining new guilds
            | GatewayIntents.GuildUsers         // Mainly when roles are added/removed to users
            | GatewayIntents.GuildPresences;    // To cache Guild Users on GUILD_CREATE for small guilds
    })
    .AddGatewayHandlers(typeof(Program).Assembly)
    ;

var host = builder
    .Build()
    .AddModules(typeof(Program).Assembly)
    .UseGatewayHandlers();

var dbContextFactory = host.Services.GetRequiredService<IDbContextFactory<BouncerBotDbContext>>();
using (var dbContext = dbContextFactory.CreateDbContext())
{
    await dbContext.Database.MigrateAsync();
}

await host.RunAsync();
