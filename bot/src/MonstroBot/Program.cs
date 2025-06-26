
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MonstroBot.Db;

using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services.AddDbContext<MonstroBotDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("MonstroBot"));
});

builder.Services
    .AddApplicationCommands()
    .AddComponentInteractions()
    .AddDiscordGateway()
    ;

IHost host = builder
    .Build()
    .AddModules(typeof(Program).Assembly)
    .UseGatewayHandlers();

await host.RunAsync();
