using BouncerBot.Attributes;
using BouncerBot.Services;

using Humanizer;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Achieve.Modules;

[ManageRolesSlashCommand(AchieveModuleMetadata.ModuleName, AchieveModuleMetadata.ModuleDescription)]
public class AchieveModule(
    IOptions<BouncerBotOptions> options,
    IAchievementService achievementService,
    IAchievementRoleOrchestrator achievementRoleOrchestrator,
    IRoleService roleService) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand(AchieveModuleMetadata.VerifyCommand.Name, AchieveModuleMetadata.VerifyCommand.Description)]
    public async Task VerifyAsync(uint hunterID, AchievementRole achievement)
    {
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        var progress = await achievementService.HasAchievementAsync(hunterID, achievement);

        var statusEmoji = progress.IsComplete ? "✅" : "❌";
        var progressText = AchievementProgressFormatter.GetProgressText(progress);

        var content = $"""
            Hunter ID: {hunterID}
            Achievement: {achievement.Humanize()}

            Status: {statusEmoji}
            {progressText}
            """;
        await ModifyResponseAsync(m =>
        {
            m.Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new (options.Value.Colors.Primary))
                    .AddComponents(
                        new TextDisplayProperties("**Achievement Status**"),
                        new ComponentSeparatorProperties().WithSpacing(ComponentSeparatorSpacingSize.Small).WithDivider(true),
                        new TextDisplayProperties(content)
                    ),
                new ActionRowProperties()
                    .AddComponents(new ButtonProperties($"achieve verify share:{content}", "Publicize", ButtonStyle.Primary))
                ];
            m.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
        });
    }

    [SubSlashCommand(AchieveModuleMetadata.GrantCommand.Name, AchieveModuleMetadata.GrantCommand.Description)]
    public async Task GrantAchievementAsync(User user, AchievementRole achievement,
        [SlashCommandParameter(Description = "Don't send an announcement to the achievement channel")] bool @private = false)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var notificationMode = @private ? NotificationMode.Silent : NotificationMode.SendMessage;
        var result = await achievementRoleOrchestrator.GrantAchievementAsync(user.Id, Context.Guild!.Id, achievement, notificationMode);

        if (result == ClaimResult.Success)
        {
            await ModifyResponseAsync(m =>
            {
                new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Success))
                    .AddTextDisplay($"Successfully granted the {achievement.Humanize()} achievement to <@{user.Id}>.")
                    .Build(m);
                m.AllowedMentions = AllowedMentionsProperties.None;
            });
        }
        else if (result == ClaimResult.AlreadyHasRole)
        {
            await ModifyResponseAsync(m =>
            {
                new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Warning))
                    .AddTextDisplay($"<@{user.Id}> already has the {achievement.Humanize()} achievement.")
                    .Build(m);
                m.AllowedMentions = AllowedMentionsProperties.None;
            });
        }
        else
        {
            throw new InvalidOperationException("Unexpected claim result: " + result);
        }
    }

    [SubSlashCommand(AchieveModuleMetadata.ResetCommand.Name, AchieveModuleMetadata.ResetCommand.Description)]
    public async Task ResetAchievementAsync(AchievementRole achievement)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        try
        {
            var numUsers = await roleService.GetRoleUserCount(Context.Guild!.Id, EnumUtils.ToRole(achievement));
            var numAchievers = await roleService.GetRoleUserCount(Context.Guild.Id, Role.Achiever);

            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Warning))
                        .AddComponents(
                            new TextDisplayProperties($"""
                                Are you sure you want to reset all the roles for {achievement.Humanize()}?

                                This will remove the role from {numUsers} users and grant the Achiever role to {numUsers - numAchievers} of them.
                                """
                            )
                        ),
                    new ActionRowProperties()
                        .AddComponents(new ButtonProperties($"achieve reset confirm:{(int)achievement}", "Confirm", ButtonStyle.Danger))
                        .AddComponents(new ButtonProperties("achieve reset cancel", "Cancel", ButtonStyle.Secondary))
                ];
                m.Flags = MessageFlags.IsComponentsV2;
            });
        }
        catch (Exception ex)
        {
            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Error))
                        .AddComponents(
                            new TextDisplayProperties($"""
                                An error occurred while preparing the reset:

                                ```
                                {ex.Message}
                                ```
                                """
                            )
                        ),
                ];
                m.Flags = MessageFlags.IsComponentsV2;
            });
        }
    }
}
