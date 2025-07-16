using BouncerBot.Attributes;
using BouncerBot.Db;
using BouncerBot.Modules.Verify;

using Humanizer;

using Microsoft.EntityFrameworkCore;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Achieve.Modules;

[SlashCommand("achieve", "Commands related to role achievements.")]
[GuildOnly<ApplicationCommandContext>]
public class AchieveModule(AchievementRoleOrchestrator achievementRoleOrchestrator, BouncerBotDbContext dbContext) : ApplicationCommandModule<ApplicationCommandContext>
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
        "I don’t see your name on the guest list. Scram!",
        "You’re not quite the big cheese we’re looking for. Move along!",
        "This club’s for the elite. Better luck next time, rookie!",
        "You’re trying to sneak in? Not on my watch!",
        "Come back when you’ve got the right moves, champ!",
        "Denied! This club’s for qualified hunters only!",
        "You’re not dressed for success. No entry!",
        "Hah! You think you can outsmart me? Not today!",
    ];

    [SlashCommand("verify", "Get an achievement role!")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Verified)]
    public async Task VerifyAsync([SlashCommandParameter(Name = "achievement")]AchievementRole role)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var mhId = (await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == Context.User.Id && vu.GuildId == Context.Guild!.Id))?.MouseHuntId;

        // Sanity check, precondition should handle this
        if (mhId is null)
        {
            await ModifyResponseAsync(m =>
            {
                m.Content = "This is a verified only club! Once you're on the list, I might let you in!";
                m.Flags = MessageFlags.Ephemeral;
            });

            return;
        }

        try
        {
            if (!await achievementRoleOrchestrator.ProcessAchievementAsync(mhId.Value, Context.User.Id, Context.Guild!.Id, role))
            {
                string randomRejectionPhrase = s_rejectionPhrases[Random.Shared.Next(s_rejectionPhrases.Length)];

                await ModifyResponseAsync(m => {
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

    [SlashCommand("reset", "Removes achievement role from all users (and grants Achiever)")]
    [ManageRolesOnly<ApplicationCommandContext>]
    public async Task ResetAchievementsAsync(AchievementRole achievement)
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Content = $"""
                Are you sure you want to reset all the roles for {achievement.Humanize()}?

                This will remove the role from all users and grant the Achiever role.

                """,
            Components = [
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"achieve reset confirm:{(int)achievement}", "Confirm", ButtonStyle.Danger))
                    .AddButtons(new ButtonProperties("achieve reset cancel", "Cancel", ButtonStyle.Secondary))
            ],
        }));
    }
}
