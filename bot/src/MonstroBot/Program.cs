
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MonstroBot;
using MonstroBot.Db;
using MonstroBot.Modules.Verify;
using MonstroBot.Rest;

using NetCord;
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
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>()
    .AddComponentInteractions<UserMenuInteraction, UserMenuInteractionContext>()
    .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>()
    .AddComponentInteractions<MentionableMenuInteraction, MentionableMenuInteractionContext>()
    .AddComponentInteractions<ChannelMenuInteraction, ChannelMenuInteractionContext>()
    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
    .AddDiscordGateway()
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
