
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);

//builder.Services.AddDbContext<MonstroBotDbContext>(options =>
//{
//    options.UseSqlite(builder.Configuration.GetConnectionString("MonstroBot"));
//});

builder.Services.AddDiscordGateway();
builder.Services.AddApplicationCommands();

IHost host = builder.Build();

//host.AddSlashCommand("ping", "Ping!", () => "Pong!")
//    .AddUserCommand("Username", (User user) => user.Username)
//    .AddMessageCommand("Length", (RestMessage message) => message.Content.Length.ToString());

host.AddModules(typeof(Program).Assembly);

host.UseGatewayHandlers();

await host.RunAsync();
