using System.Security.Cryptography;
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
using BouncerBot.TypeReaders;
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

// Diagnostics and monitoring
if (!string.IsNullOrEmpty(builder.Configuration["AzureMonitorExporter:ConnectionString"]))
{
    // Adds an IHostedService that needs to be started first
    builder.Services.AddOpenTelemetry().UseAzureMonitorExporter();
}
if (!string.IsNullOrEmpty(builder.Configuration["Sentry:Dsn"]))
{
    builder.Logging.AddConfiguration(builder.Configuration);
    builder.Logging.AddSentry();
}

// BouncerBot services
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
    .AddTransient<IMouseHuntEmojiService, MouseHuntEmojiService>()
    .AddTransient<IHashService, HMACSHA3HashService>()
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
    .AddHttpClient<MouseRipService>();

builder.Services.AddMemoryCache();

// NetCord services
builder.Services
    .AddSingleton<IdApplicationCommandServiceStorage<ApplicationCommandContext>>()
    .AddApplicationCommands((options, services) =>
    {
        // Custom type readers so emojis options on slash commands can be searched by name or emoji
        options.TypeReaders.Add(typeof(AchievementRole), new HumanizedEnumTypeReader<AchievementRole>());
        options.TypeReaders.Add(typeof(BouncerBot.Role), new HumanizedEnumTypeReader<BouncerBot.Role>());

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
    .AddModules(typeof(Program).Assembly);

var dbContextFactory = host.Services.GetRequiredService<IDbContextFactory<BouncerBotDbContext>>();
using (var dbContext = dbContextFactory.CreateDbContext())
{
    await dbContext.Database.MigrateAsync();

    var hashingService = host.Services.GetRequiredService<IHashService>();
    foreach (var user in dbContext.VerifiedUsers)
    {
        var oldDiscordIdHash = HashValue(user.DiscordId);
        var oldMouseHuntIdHash = HashValue(user.MouseHuntId);
        var mhIdHash = hashingService.HashValue(user.MouseHuntId);
        var discordIdHash = hashingService.HashValue(user.DiscordId);

        foreach (var verification in dbContext.VerificationHistory.Where(vh => vh.DiscordIdHash == oldDiscordIdHash))
        {
            dbContext.VerificationHistory.Remove(verification);

            dbContext.VerificationHistory.Add(new BouncerBot.Db.Models.VerificationHistory
            {
                MouseHuntIdHash = mhIdHash,
                DiscordIdHash = discordIdHash,
                GuildId = verification.GuildId,
                CreatedAt = verification.CreatedAt,
            });
        }
    }

    await dbContext.SaveChangesAsync();

    static string HashValue(ulong value)
    {
        var bytes = SHA512.HashData(BitConverter.GetBytes(value));
        return Convert.ToBase64String(bytes);
    }
}

await host.RunAsync();
