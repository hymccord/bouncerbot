using BouncerBot.Modules.Verification;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.Verify.Modules;
public class VerifyButtonInteractions(
    IOptionsSnapshot<BouncerBotOptions> options,
    IVerificationOrchestrator verificationOrchestrator) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("link start")]
    public async Task VerifyMe(ulong guildId, uint mouseHuntId, string phrase)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        await ModifyResponseAsync(x =>
        {
            x.Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new (options.Value.Colors.Primary))
                    .AddComponents(
                        new TextDisplayProperties("Please wait while I read your profile...")
                    )
                ];
            x.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
        });

        var verificationParameters = new VerificationParameters
        {
            MouseHuntId = mouseHuntId,
            DiscordUserId = Context.User.Id,
            GuildId = guildId,
            Phrase = phrase,
        };

        try
        {
            var verificationResult = await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Self, verificationParameters);

            await ModifyResponseAsync(x =>
            {
                x.Components = [
                    new ComponentContainerProperties()
                    .WithAccentColor(new (verificationResult.Success ? options.Value.Colors.Success : options.Value.Colors.Error))
                    .AddComponents(
                        new TextDisplayProperties(verificationResult.Message)
                    )
                    ];
                x.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });
        }
        catch (RestException ex) when (ex is { StatusCode: System.Net.HttpStatusCode.Forbidden, Error: RestError error })
        {
            string message;
            switch (error.Code)
            {
                case 50001: // Missing Access
                    message = "I do not have access to the appropriate channel to send your achievement, but I have awarded the role.";
                    break;
                case 50013: // Lack permissions
                    message = "I do not have permission add the appropriate role. Please contact a server administrator.";
                    break;
                default:
                    throw;
            }

            await ModifyResponseAsync(x =>
            {
                x.Components = [
                    new ComponentContainerProperties()
                    .WithAccentColor(new (options.Value.Colors.Error))
                    .AddComponents(
                        new TextDisplayProperties(message)
                    )
                ];
                x.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });
        }
    }

    [ComponentInteraction("link cancel")]
    public async Task CancelVerification()
    {
        await DeferModifyAndDeleteResponseAsync();
    }

    private async Task DeferModifyAndDeleteResponseAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        await DeleteResponseAsync();
    }
}
