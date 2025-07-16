using Microsoft.Extensions.Hosting;

namespace BouncerBot.Rest;

internal class MouseHuntRestClientHostedService(MouseHuntRestClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.StartAsync(cancellationToken);
        await client.GetTitlesAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopAsync(cancellationToken);
    }
}
