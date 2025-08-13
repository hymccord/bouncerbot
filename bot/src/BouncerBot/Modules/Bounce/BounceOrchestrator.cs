using BouncerBot.Db;
using BouncerBot.Modules.Verification;
using BouncerBot.Services;

using Microsoft.EntityFrameworkCore;

using NetCord;
using NetCord.Rest;

namespace BouncerBot.Modules.Bounce;

public interface IBounceOrchestrator
{
    Task<BounceResult> AddBannedHunterAsync(uint hunterId, ulong guildId, string? note = null);
    Task<BounceResult> RemoveBannedHunterAsync(uint hunterId, ulong guildId);
    Task<BounceResult> RemoveAllBannedHuntersAsync(ulong guildId);
    Task<BounceResult> UpdateBannedHunterNoteAsync(uint hunterId, ulong guildId, string? note = null);
    Task<BounceResult> CheckBannedHunterAsync(uint hunterId, ulong guildId);
}

public class BounceOrchestrator(
    IBounceService bounceService,
    IGuildLoggingService guildLoggingService,
    IVerificationOrchestrator verificationOrchestrator,
    BouncerBotDbContext dbContext) : IBounceOrchestrator
{
    public async Task<BounceResult> AddBannedHunterAsync(uint hunterId, ulong guildId, string? note = null)
    {
        try
        {
            if (await bounceService.IsHunterBannedAsync(hunterId, guildId))
            {
                return new BounceResult
                {
                    Success = false,
                    Message = $"Hunter ID {hunterId} is already banned."
                };
            }

            await bounceService.AddBannedHunterAsync(hunterId, guildId, note);

            // Shouldn't happen much, but if the hunter is already verified, we need to remove their verification
            if (await dbContext.VerifiedUsers.FirstOrDefaultAsync(vu => vu.MouseHuntId == hunterId && vu.GuildId == guildId) is { } verifiedUser)
            {
                await verificationOrchestrator.ProcessVerificationAsync(VerificationType.Remove, new VerificationParameters
                {
                    GuildId = guildId,
                    DiscordUserId = verifiedUser.DiscordId,
                });
            }

            // Log the ban action
            await guildLoggingService.LogAsync(guildId, LogType.General, new MessageProperties
            {
                Embeds = [
                    new EmbedProperties() {
                        Title = "Hunter Bounced",
                        Color = new Color(0xdd5ee3),
                        Timestamp = DateTime.UtcNow,
                        Description = $"""
                        **Hunter ID**: {hunterId}
                        **Note**: {(note ?? "")}
                        """
                    }],
            });

            return new BounceResult
            {
                Success = true,
                Message = $"""
                    ✅ Successfully banned Hunter ID {hunterId} from using `/link`.
                    {(note != null ? $"Note: {note}" : "")}
                    """
            };
        }
        catch (Exception ex)
        {
            return new BounceResult
            {
                Success = false,
                Message = $"An error occurred while adding the ban: {ex.Message}"
            };
        }
    }

    public async Task<BounceResult> RemoveBannedHunterAsync(uint hunterId, ulong guildId)
    {
        try
        {
            var bannedHunter = await bounceService.GetBannedHunterAsync(hunterId, guildId);
            if (bannedHunter == null)
            {
                return new BounceResult
                {
                    Success = false,
                    Message = $"Hunter ID {hunterId} is not currently banned."
                };
            }

            await bounceService.RemoveBannedHunterAsync(hunterId, guildId);

            // Log the unban action
            await guildLoggingService.LogAsync(guildId, LogType.General, new MessageProperties
            {
                Content = $"Hunter ID {hunterId} was unbanned and can now use `/link`",
            });

            return new BounceResult
            {
                Success = true,
                Message = $"✅ Successfully removed Hunter ID {hunterId} from the ban list."
            };
        }
        catch (Exception ex)
        {
            return new BounceResult
            {
                Success = false,
                Message = $"An error occurred while removing the ban: {ex.Message}"
            };
        }
    }

    public async Task<BounceResult> RemoveAllBannedHuntersAsync(ulong guildId)
    {
        try
        {
            await bounceService.RemoveAllBannedHuntersAsync(guildId);
            await guildLoggingService.LogAsync(guildId, LogType.General, new MessageProperties
            {
                Content = $"All banned hunters have been removed from the ban list."
            });

            return new BounceResult
            {
                Success = true,
                Message = $"✅ Successfully removed all banned hunters from the ban list."
            };
        }
        catch (Exception ex)
        {
            return new BounceResult
            {
                Success = false,
                Message = $"An error occurred while removing all hunter bans: {ex.Message}"
            };
        }
    }

    public async Task<BounceResult> UpdateBannedHunterNoteAsync(uint hunterId, ulong guildId, string? note = null)
    {
        try
        {
            var bannedHunter = await bounceService.GetBannedHunterAsync(hunterId, guildId);
            if (bannedHunter == null)
            {
                return new BounceResult
                {
                    Success = false,
                    Message = $"Hunter ID {hunterId} is not currently banned."
                };
            }

            await bounceService.UpdateBannedHunterNoteAsync(hunterId, guildId, note);

            return new BounceResult
            {
                Success = true,
                Message = string.IsNullOrWhiteSpace(note) 
                    ? $"✅ Removed note for Hunter ID {hunterId}."
                    : $"✅ Updated note for Hunter ID {hunterId}: {note}"
            };
        }
        catch (Exception ex)
        {
            return new BounceResult
            {
                Success = false,
                Message = $"An error occurred while updating the note: {ex.Message}"
            };
        }
    }

    public async Task<BounceResult> CheckBannedHunterAsync(uint hunterId, ulong guildId)
    {
        try
        {
            var bannedHunter = await bounceService.GetBannedHunterAsync(hunterId, guildId);
            
            if (bannedHunter == null)
            {
                return new BounceResult
                {
                    Success = true,
                    Message = $"✅ Hunter ID {hunterId} is **not banned** and can use `/link`."
                };
            }

            return new BounceResult
            {
                Success = true,
                Message = $"""
                    ❌ Hunter ID {hunterId} is **banned** from using `/link`.
                    
                    Banned <t:{((DateTimeOffset)bannedHunter.CreatedAt).ToUnixTimeSeconds()}:F>
                    {(string.IsNullOrWhiteSpace(bannedHunter.Note) ? "" : $"Note: {bannedHunter.Note}")}
                    """,
            };
        }
        catch (Exception ex)
        {
            return new BounceResult
            {
                Success = false,
                Message = $"An error occurred while checking the ban status: {ex.Message}"
            };
        }
    }
}

public readonly record struct BounceResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
}
