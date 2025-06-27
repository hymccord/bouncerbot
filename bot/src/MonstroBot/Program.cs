
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MonstroBot.Db;

using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services.AddDbContextFactory<MonstroBotDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("MonstroBot"));
});

builder.Services
    .AddApplicationCommands()
    //.AddApplicationCommands<SlashCommandInteraction, SlashCommandContext, AutocompleteInteractionContext>()
    //.AddComponentInteractions()
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
