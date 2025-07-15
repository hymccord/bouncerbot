
using BouncerBot;
using BouncerBot.Db;
using BouncerBot.Modules.Achieve;
using BouncerBot.Modules.Config;
using BouncerBot.Modules.Verify;
using BouncerBot.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services
    .AddDbContextFactory<BouncerBotDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("BouncerBot"));
    })
    .AddTransient<IGuildLoggingService, GuildLoggingService>()
    .AddTransient<IGuildUserRoleMonitorService, GuildUserRoleMonitor>()
    .AddTransient<IVerificationPhraseGenerator, VerificationPhraseGenerator>()
    .AddTransient<AchievementMessageService>()
    .AddTransient<AchievementRoleService>()
    .AddTransient<AchievementRoleOrchestrator>()
    .AddTransient<AchievementService>()
    .AddTransient<ConfigService>()
    .AddTransient<VerificationService>()
    .AddMouseHuntClient()
    ;

// NetCord services
builder.Services
    .AddApplicationCommands(options =>
    {
        options.ResultHandler = new EphemeralApplicationCommandResultHandler();
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
            Activities = [new ("MouseHunters!", UserActivityType.Watching)]
        };
        options.Intents = GatewayIntents.Guilds
            | GatewayIntents.GuildUsers
            | GatewayIntents.GuildPresences;
    })
    .AddGatewayHandlers(typeof(Program).Assembly)
    ;

IHost host = builder
    .Build()
    .AddModules(typeof(Program).Assembly)
    .UseGatewayHandlers();

using (var dbContext = host.Services.GetRequiredService<BouncerBotDbContext>())
{
    await dbContext.Database.MigrateAsync();
}

await host.RunAsync();
