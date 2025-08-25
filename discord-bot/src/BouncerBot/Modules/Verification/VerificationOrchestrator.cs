using BouncerBot;
using BouncerBot.Rest;
using BouncerBot.Services;
using NetCord;
using NetCord.Rest;

namespace BouncerBot.Modules.Verification;

public interface IVerificationOrchestrator
{
    Task<VerificationResult> ProcessVerificationAsync(VerificationType type, VerificationParameters parameters, CancellationToken cancellationToken = default);
}

public class VerificationOrchestrator(
    IGuildLoggingService guildLoggingService,
    IMouseHuntRestClient mouseHuntRestClient,
    IRoleService roleService,
    IVerificationService verificationService
) : IVerificationOrchestrator
{
    public async Task<VerificationResult> ProcessVerificationAsync(VerificationType type, VerificationParameters parameters, CancellationToken cancellationToken = default)
        => type switch
        {
            VerificationType.Self => await DoSelfVerificationAsync(parameters, cancellationToken),
            VerificationType.Add => await DoAddVerificationAsync(parameters, cancellationToken),
            VerificationType.Remove => await DoRemoveVerificationAsync(parameters, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown verification type."),
        };

    private async Task<VerificationResult> DoSelfVerificationAsync(VerificationParameters parameters, CancellationToken cancellationToken = default)
    {
        var snuid = await mouseHuntRestClient.GetUserSnuIdAsync(parameters.MouseHuntId, cancellationToken);
        var latestMessage = (await mouseHuntRestClient.GetCorkboardAsync(parameters.MouseHuntId, cancellationToken)).CorkboardMessages.FirstOrDefault();

        var snuidMatch = snuid.SnUserId == latestMessage?.SnUserId;
        var phraseMatch = latestMessage?.Body == parameters.Phrase;
        if (snuidMatch && phraseMatch)
        {
            var result = await verificationService.AddVerifiedUserAsync(parameters.MouseHuntId, parameters.GuildId, parameters.DiscordUserId, cancellationToken);

            if (!result.WasAdded)
            {
                return new VerificationResult
                {
                    Success = false,
                    Message = $"Sorry! Something went wrong when processing your self-verification.",
                };
            }

            await roleService.AddRoleAsync(parameters.DiscordUserId, parameters.GuildId, Role.Verified, cancellationToken);
            var message = await LogVerificationAsync(parameters, result.MouseHuntId, cancellationToken);
            if (message is not null)
            {
                await verificationService.SetVerificationMessageAsync(new()
                {
                    GuildId = parameters.GuildId,
                    DiscordUserId = parameters.DiscordUserId,
                    ChannelId = message.Value.ChannelId,
                    MessageId = message.Value.Id,
                });
            }

            return new VerificationResult
            {
                Success = true,
                Message = "Success! You may now remove the phrase from your corkboard.",
            };
        }

        if (phraseMatch && !snuidMatch)
        {
            await guildLoggingService.LogWarningAsync(parameters.GuildId,
                title: "Verification Blocked",
                content: $"""
                :warning: <@{parameters.DiscordUserId}> wrote a verification phrase on a profile that isn't theirs. :warning:
                Profile SnuId: {snuid.SnUserId}, Corkboard Author SnuId: {latestMessage?.SnUserId}",
                """, cancellationToken);
        }

        return new VerificationResult
        {
            Success = false,
            Message = $"""
                Linking failed!

                Please ensure that you have the correct phrase on your corkboard.
                The latest message on your corkboard is:

                ```
                {latestMessage?.Body ?? "No messages found."}
                ```
                """
        };
    }

    private async Task<VerificationResult> DoAddVerificationAsync(VerificationParameters parameters, CancellationToken cancellationToken)
    {
        if (!await verificationService.IsDiscordUserVerifiedAsync(parameters.GuildId, parameters.DiscordUserId, cancellationToken))
        {
            var result = await verificationService.AddVerifiedUserAsync(parameters.MouseHuntId, parameters.GuildId, parameters.DiscordUserId, cancellationToken);

            if (!result.WasAdded)
            {
                return new VerificationResult
                {
                    Success = false,
                    Message = $"Sorry! Something went wrong when processing the verification for <@{parameters.DiscordUserId}>.",
                };
            }

            await roleService.AddRoleAsync(parameters.DiscordUserId, parameters.GuildId, Role.Verified, cancellationToken);
            var message = await LogVerificationAsync(parameters, result.MouseHuntId, cancellationToken);
            if (message is not null)
            {
                await verificationService.SetVerificationMessageAsync(new()
                {
                    GuildId = parameters.GuildId,
                    DiscordUserId = parameters.DiscordUserId,
                    ChannelId = message.Value.ChannelId,
                    MessageId = message.Value.Id,
                });
            }

            return new VerificationResult
            {
                Success = true,
                Message = $"Verified <@{parameters.DiscordUserId}> to be hunter {parameters.MouseHuntId}.",
            };
        }
        else
        {
            return new VerificationResult
            {
                Success = false,
                Message = $"<@{parameters.DiscordUserId}> is already verified.",
            };
        }
    }

    private async Task<VerificationResult> DoRemoveVerificationAsync(VerificationParameters parameters, CancellationToken cancellationToken)
    {
        if (await verificationService.IsDiscordUserVerifiedAsync(parameters.GuildId, parameters.DiscordUserId, cancellationToken))
        {
            _ = await verificationService.RemoveVerifiedUser(parameters.GuildId, parameters.DiscordUserId, cancellationToken);
            await roleService.RemoveRoleAsync(parameters.DiscordUserId, parameters.GuildId, Role.Verified, cancellationToken);

            return new VerificationResult
            {
                Success = true,
                Message = $"Removed verification for <@{parameters.DiscordUserId}>.",
            };
        }
        else
        {
            return new VerificationResult
            {
                Success = false,
                Message = $"<@{parameters.DiscordUserId}> is not verified.",
            };
        }
    }

    private async Task<(ulong ChannelId, ulong Id)?> LogVerificationAsync(VerificationParameters parameters, uint hunterId, CancellationToken cancellationToken = default)
    {
        return await guildLoggingService.LogAsync(parameters.GuildId, LogType.Verification, new()
        {
            Components = [
                new ComponentContainerProperties()
                    .AddComponents(
                        new TextDisplayProperties($"<@{parameters.DiscordUserId}> {parameters.DiscordUserId} is hunter [{hunterId}](<https://p.mshnt.ca/{hunterId}>)")
                    )
                ],
            AllowedMentions = AllowedMentionsProperties.None,
            Flags = MessageFlags.IsComponentsV2
        }, cancellationToken);
    }
}

public readonly record struct VerificationParameters
{
    public uint MouseHuntId { get; init; }
    public required ulong DiscordUserId { get; init; }
    public required ulong GuildId { get; init; }
    public string? Phrase { get; init; }
}

public enum VerificationType
{
    Self,
    Add,
    Remove,
}

public record VerificationResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
}
