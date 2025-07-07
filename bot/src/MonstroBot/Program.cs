
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MonstroBot;
using MonstroBot.Db;
using MonstroBot.Modules.Verify;
using MonstroBot.Services;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services
    .AddDbContextFactory<MonstroBotDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("MonstroBot"));
    })
    .AddTransient<IGuildLoggingService, GuildLoggingService>()
    .AddTransient<IGuildUserRoleMonitorService, GuildUserRoleMonitor>()
    .AddTransient<IVerificationPhraseGenerator, VerificationPhraseGenerator>()
    .AddTransient<VerificationService>()
    .AddMouseHuntClient()
    ;

// NetCord services
builder.Services
    .AddApplicationCommands(options =>
    {
        options.ResultHandler = new EphemeralApplicationCommandResultHandler();
    })
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>(options =>
    {
        options.ResultHandler = new EphemeralComponentInteractionResultHandler<ButtonInteractionContext>();
    })
    .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>(options =>
    {
        options.ResultHandler = new EphemeralComponentInteractionResultHandler<StringMenuInteractionContext>();
    })
    .AddComponentInteractions<UserMenuInteraction, UserMenuInteractionContext>(options =>
    {
        options.ResultHandler = new EphemeralComponentInteractionResultHandler<UserMenuInteractionContext>();
    })
    .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>(options =>
    {
        options.ResultHandler = new EphemeralComponentInteractionResultHandler<RoleMenuInteractionContext>();
    })
    .AddComponentInteractions<MentionableMenuInteraction, MentionableMenuInteractionContext>(options =>
    {
        options.ResultHandler = new EphemeralComponentInteractionResultHandler<MentionableMenuInteractionContext>();
    })
    .AddComponentInteractions<ChannelMenuInteraction, ChannelMenuInteractionContext>(options =>
    {
        options.ResultHandler = new EphemeralComponentInteractionResultHandler<ChannelMenuInteractionContext>();
    })
    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>(options =>
    {
        options.ResultHandler = new EphemeralComponentInteractionResultHandler<ModalInteractionContext>();
    })
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

using (var dbContext = host.Services.GetRequiredService<MonstroBotDbContext>())
{
    await dbContext.Database.MigrateAsync();
}

await host.RunAsync();
