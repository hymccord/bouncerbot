using BouncerBot.Modules.Verification;

using Microsoft.Extensions.DependencyInjection;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace BouncerBot.GatewayHandlers;

/// <summary>
/// Handles the event when a user is removed from a guild.
/// </summary>
/// <remarks>This handler processes the removal of a user from a guild by initiating a verification process
/// through the <see cref="VerificationOrchestrator"/>. It is designed to be used in scenarios where user removal needs
/// to trigger specific verification actions.</remarks>
/// <param name="verificationOrchestrator"></param>
public class GuildUserRemoveGatewayHandler(IServiceScopeFactory serviceScopeFactory) : IGuildUserRemoveGatewayHandler
{
    public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var verificationOrchestrator = scope.ServiceProvider.GetRequiredService<IVerificationOrchestrator>();

            await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Remove, new VerificationParameters()
            {
                DiscordUserId = arg.User.Id,
                GuildId = arg.GuildId,
            });
        }
    }
}
