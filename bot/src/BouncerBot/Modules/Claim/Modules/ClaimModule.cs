using BouncerBot.Attributes;
using BouncerBot.Db;
using BouncerBot.Modules.Achieve;
using BouncerBot.Modules.Verification;
using BouncerBot.Services;

using Microsoft.EntityFrameworkCore;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Claim.Modules;

[RequireGuildContext<ApplicationCommandContext>]
public class ClaimModule(
    IAchievementRoleOrchestrator achievementRoleOrchestrator,
    ICommandMentionService commandMentionService,
    BouncerBotDbContext dbContext) : ApplicationCommandModule<ApplicationCommandContext>
{
    private static readonly string[] s_rejectionPhrases = [
        "Hah, trying to pull a fast one on me!? Scram!",
        "Not on the list, not in the club! Try again, pal.",
        "You think you can cheese your way in here? Think again!",
        "Sorry, no entry for hunters without the right credentials!",
        "You’re not VIP material yet. Come back when you’ve earned it!",
        "Nice try, but this club’s for achievers only!",
        "You’re squeaking up the wrong door, buddy!",
        "No badge, no boogie. Rules are rules!",
        "I don’t see your name on the VIP list. Scram!",
        "You’re not quite the big cheese we’re looking for. Move along!",
        "This club’s for the elite. Better luck next time, rookie!",
        "You’re trying to sneak in? Not on my watch!",
        "Come back when you’ve got the right moves, champ!",
        "Denied! This club’s for qualified hunters only!",
        "You’re not dressed for success. No entry!",
        "Hah! You think you can outsmart me? Not today!",
    ];

    [SlashCommand("claim", "Claim an achievement role!")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Verified)]
    public async Task ClaimAsync(AchievementRole achievement,
        [SlashCommandParameter(Description = "Publicly share your achievement? (Defaults to true)")]bool? share = true)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var mhId = (await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == Context.User.Id && vu.GuildId == Context.Guild!.Id))?.MouseHuntId;

        // Sanity check, precondition should handle this
        if (mhId is null)
        {
            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                This is a verified only club! Once you're on the list, I might let you in!

                -# Hint: You can use the {commandMentionService.GetCommandMention("link")} command to verify your account.
                """;
                m.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        try
        {
            var achieved = (share ?? true)
                ? achievementRoleOrchestrator.ProcessAchievementAsync(mhId.Value, Context.User.Id, Context.Guild!.Id, achievement)
                : achievementRoleOrchestrator.ProcessAchievementSilentlyAsync(mhId.Value, Context.User.Id, Context.Guild!.Id, achievement);
            if (!await achieved)
            {
                string randomRejectionPhrase = s_rejectionPhrases[Random.Shared.Next(s_rejectionPhrases.Length)];

                await ModifyResponseAsync(m =>
                {
                    m.Content = randomRejectionPhrase;
                    m.Flags = MessageFlags.Ephemeral;
                });
            }
        }
        catch (Exception ex)
        {
            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                    An error occurred while processing your achievement. Please try again later.

                    Error: `{ex.Message}`
                    """;
                m.Flags = MessageFlags.Ephemeral;
            });
        }
    }
}
