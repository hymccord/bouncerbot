using BouncerBot.Modules.Verification;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
public class GuildUserRemoveGatewayHandler(
    ILogger<GuildUserRemoveGatewayHandler> logger,
    IServiceScopeFactory serviceScopeFactory
) : IGuildUserRemoveGatewayHandler
{
    public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var verificationOrchestrator = scope.ServiceProvider.GetRequiredService<IVerificationOrchestrator>();

            try
            {
                await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Remove, new VerificationParameters()
                {
                    DiscordUserId = arg.User.Id,
                    GuildId = arg.GuildId,
                });

            } catch (InvalidOperationException ex)
            {
                logger.LogInformation(ex, """
                    Verification process could not be completed for user {UserId} in guild {GuildId} during removal.

                    This is expected behavior if the user was kicked or banned.
                    """, arg.User.Id, arg.GuildId);
            }
        }
    }
}
