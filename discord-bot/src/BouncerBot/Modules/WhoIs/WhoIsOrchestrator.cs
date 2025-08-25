namespace BouncerBot.Modules.WhoIs;

public interface IWhoIsOrchestrator
{
    Task<WhoIsResult> GetHunterIdAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default);
    Task<WhoIsResult> GetUserIdAsync(ulong guildId, uint mouseHuntId, CancellationToken cancellationToken = default);
}

public class WhoIsOrchestrator(IWhoIsService whoIsService) : IWhoIsOrchestrator
{
    public async Task<WhoIsResult> GetHunterIdAsync(ulong guildId, ulong discordId, CancellationToken cancellationToken = default)
    {
        try
        {
            var verifiedUser = await whoIsService.GetVerifiedUserByDiscordIdAsync(guildId, discordId, cancellationToken);
            
            if (verifiedUser == null)
            {
                return new WhoIsResult
                {
                    Success = false,
                    Message = $"<@{discordId}> is not verified.",
                };
            }

            return new WhoIsResult
            {
                Success = true,
                Message = $"<@{discordId}> is hunter **[{verifiedUser.MouseHuntId}](<https://p.mshnt.ca/{verifiedUser.MouseHuntId}>)**.",
            };
        }
        catch (Exception ex)
        {
            return new WhoIsResult
            {
                Success = false,
                Message = $"An error occurred while looking up the user: {ex.Message}."
            };
        }
    }

    public async Task<WhoIsResult> GetUserIdAsync(ulong guildId, uint mouseHuntId, CancellationToken cancellationToken = default)
    {
        try
        {
            var verifiedUser = await whoIsService.GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId, cancellationToken);
            
            if (verifiedUser == null)
            {
                return new WhoIsResult
                {
                    Success = false,
                    Message = $"Hunter **[{mouseHuntId}](<https://p.mshnt.ca/{mouseHuntId}>)** is not verified."
                };
            }

            return new WhoIsResult
            {
                Success = true,
                Message = $"Hunter **[{mouseHuntId}](<https://p.mshnt.ca/{mouseHuntId}>)** is <@{verifiedUser.DiscordId}>.",
            };
        }
        catch (Exception ex)
        {
            return new WhoIsResult
            {
                Success = false,
                Message = $"An error occurred while looking up the hunter: {ex.Message}."
            };
        }
    }
}

public readonly record struct WhoIsResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
}
