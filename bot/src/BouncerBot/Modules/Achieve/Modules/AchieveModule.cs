using BouncerBot.Attributes;

using Humanizer;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Achieve.Modules;

[SlashCommand("achieve", "Commands related to role achievements.")]
[RequireGuildContext<ApplicationCommandContext>]
public class AchieveModule(
    AchievementService achievementService,
    AchievementRoleService achievementRoleService) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("verify", "Check if a Hunter ID qualifies for an achievement.")]
    [RequireManageRoles<ApplicationCommandContext>]
    public async Task VerifyAsync(uint hunterID, AchievementRole achievement)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        bool hasAchievement = await achievementService.HasAchievementAsync(hunterID, achievement);

        var content = $"""
            Hunter ID: {hunterID}
            Achievement: {achievement.Humanize()}

            Status: {(hasAchievement ? "✅" : "❌")}
            """;
        await ModifyResponseAsync(m =>
        {
            m.Content = content;
            m.Components = [
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"achieve verify share:{content}", "Make Public", ButtonStyle.Primary))
            ];
        });
    }

    [SubSlashCommand("reset", "Removes achievement role from all users (and grants Achiever)")]
    [RequireManageRoles<ApplicationCommandContext>]
    public async Task ResetAchievementAsync(AchievementRole achievement)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        try
        {
            var numUsers = await achievementRoleService.CountUsersInRole(Context.Guild!.Id, EnumUtils.ToRole(achievement));
            var numAchievers = await achievementRoleService.CountUsersInRole(Context.Guild.Id, Role.Achiever);

            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                    Are you sure you want to reset all the roles for {achievement.Humanize()}?

                    This will remove the role from {numUsers} users and grant the Achiever role to {numUsers - numAchievers} of them.
                    """;

                m.Components = [
                    new ActionRowProperties()
                        .AddButtons(new ButtonProperties($"achieve reset confirm:{(int)achievement}", "Confirm", ButtonStyle.Danger))
                        .AddButtons(new ButtonProperties("achieve reset cancel", "Cancel", ButtonStyle.Secondary))
                ];
            });
        }
        catch (Exception ex)
        {
            await ModifyResponseAsync(m =>
            {
                m.Content = $"""
                    An error occurred while preparing the reset:

                    ```
                    {ex.Message}
                    ```
                    """;
                
                m.Components = [];
            });
        }
    }
}
