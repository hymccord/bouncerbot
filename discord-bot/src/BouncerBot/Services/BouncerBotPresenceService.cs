using NetCord;
using NetCord.Gateway;

namespace BouncerBot.Services;

public interface IBouncerBotPresenceService
{
    Task SetDefaultPresence();
    Task SetPuzzlePresence();
}

internal class BouncerBotPresenceService(
    GatewayClient gatewayClient
    ) : IBouncerBotPresenceService
{

    public async Task SetDefaultPresence()
    {
        await gatewayClient.UpdatePresenceAsync(new PresenceProperties(UserStatusType.Online)
        {
            Activities = [new("MouseHunters!", UserActivityType.Watching)]
        });
    }

    public async Task SetPuzzlePresence()
    {
        await gatewayClient.UpdatePresenceAsync(new PresenceProperties(UserStatusType.DoNotDisturb)
        {
            Activities = [new("with a puzzle... 🤖", UserActivityType.Playing)]
        });
    }
}
