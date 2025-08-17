using BouncerBot.Attributes;
using BouncerBot.Db;
using BouncerBot.Modules.Achieve;
using BouncerBot.Modules.Verification;
using BouncerBot.Modules.Verify.Modules;
using BouncerBot.Rest;
using BouncerBot.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Claim.Modules;

[RequireGuildContext<ApplicationCommandContext>]
public class ClaimModule(
    ILogger<ClaimModule> logger,
    IOptions<BouncerBotOptions> options,
    IAchievementRoleOrchestrator achievementRoleOrchestrator,
    ICommandMentionService commandMentionService,
    BouncerBotDbContext dbContext) : ApplicationCommandModule<ApplicationCommandContext>
{
    private static readonly string[] s_rejectionPhrases = [
        "Hah, trying to pull a fast one on me!? Scram!",
        "You think you can cheese your way in here? Think again!",
        "Nice try, but this club is for real achievers only!",
        "You're not quite the big cheese we're looking for. Move along!",
        "This club's for the elite. Keep working on it, rookie!",
        "You're trying to sneak in to this exclusive club without all the work? Not on my watch!",
        "Come back when you've got all the qualifications, champ!",
        "Denied. This club is for qualified hunters only!",
        "Hah! You think you can outsmart me by not completing all the requirements? Not today!",
    ];

    [SlashCommand(ClaimModuleMetadata.ClaimCommand.Name, ClaimModuleMetadata.ClaimCommand.Description)]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Verified)]
    public async Task ClaimAsync(AchievementRole achievement,
        [SlashCommandParameter(Description = "Keep private? (Don't send announcement)")]bool? @private = false)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var mhId = (await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == Context.User.Id && vu.GuildId == Context.Guild!.Id))?.MouseHuntId;

        var container = new ComponentContainerProperties();
        
        // Sanity check, precondition should handle this
        if (mhId is null)
        {
            await ModifyResponseAsync(m =>
            {
                m.Components = [container
                    .WithAccentColor(new(options.Value.Colors.Warning))
                    .AddComponents(
                    new TextDisplayProperties($"""
                        This is a verified only club! Once you're on the list, I might let you in!

                        -# Hint: You can use the {commandMentionService.GetCommandMention(VerifyModuleMetadata.VerifyCommand.Name)} command to verify your account.
                        """
                    ))];
                m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
            });

            return;
        }

        try
        {
            var achieved = await ((@private ?? false)
                ? achievementRoleOrchestrator.ProcessAchievementSilentlyAsync(mhId.Value, Context.User.Id, Context.Guild!.Id, achievement)
                : achievementRoleOrchestrator.ProcessAchievementAsync(mhId.Value, Context.User.Id, Context.Guild!.Id, achievement));
            switch (achieved)
            {
                case ClaimResult.NotAchieved:
                    {
                        var randomRejectionPhrase = s_rejectionPhrases[Random.Shared.Next(s_rejectionPhrases.Length)];

                        await ModifyResponseAsync(m =>
                        {
                            m.Components = [container
                                .WithAccentColor(new(options.Value.Colors.Error))
                                .AddComponents(
                                new TextDisplayProperties($"""
                                {randomRejectionPhrase}

                                -# Hint: You are missing some requirements to claim this achievement. Make sure you have completed all the necessary tasks before trying again.
                                """
                            ))];
                            m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
                        });
                        break;
                    }

                case ClaimResult.AlreadyHasRole:
                    {
                        await ModifyResponseAsync(m =>
                        {
                            m.Components = [container
                                .WithAccentColor(new(options.Value.Colors.Warning))
                                .AddComponents(
                                new TextDisplayProperties($"""
                                You've already claimed this achievement! No need to do it again.

                                -# {(@private ?? false ? "And don't worry, I won't tell anyone you tried!" : "Glad to see you're proud of your achievements!")}
                                """
                            ))];
                            m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
                        });
                        break;
                    }
                case ClaimResult.Success:
                    {
                        await ModifyResponseAsync(m =>
                        {
                            m.Components = [container
                                .WithAccentColor(new(options.Value.Colors.Success))
                                .AddComponents(
                                new TextDisplayProperties($"""
                                    Congratulations! I've checked your profile and you meet the requirements.

                                    I've awarded the role and {(@private ?? false ? "kept it our little secret!" : "shared it with everyone!")}
                                    """
                            ))];
                            m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
                        });
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unknown ClaimResult value.");
            }
        }
        catch (RoleNotConfiguredException ex)
        {
            logger.LogInformation(ex, "Role not configured for achievement {Achievement} in guild {Guild}", achievement, Context.Guild!.Id);

            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                    The role for this achievement has not been configured.

                    Contact the server moderators to resolve this issue.
                    """;
                m.Flags = MessageFlags.Ephemeral;
            });
        }
        catch (MessageNotConfiguredException ex)
        {
            logger.LogInformation(ex, "Message not configured for achievement {Achievement} in guild {Guild}", achievement, Context.Guild!.Id);

            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                    The message for this achievement is not configured properly, but the role has been assigned successfully.

                    Contact the server moderators to resolve this issue.
                    """;
                m.Flags = MessageFlags.Ephemeral;
            });
        }
        catch (PuzzleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                    An error occurred while processing your achievement. Please try again later.

                    -# Error: {ex.Message}
                    """;
                m.Flags = MessageFlags.Ephemeral;
            });
        }
    }
}
