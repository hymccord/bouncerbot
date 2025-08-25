using Microsoft.Extensions.Hosting;

namespace BouncerBot.Rest;

internal class MouseHuntRestClientHostedService(IMouseHuntRestClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopAsync(cancellationToken);
    }
}
