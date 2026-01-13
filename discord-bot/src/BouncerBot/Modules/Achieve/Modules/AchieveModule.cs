using BouncerBot.Attributes;
using BouncerBot.Services;

using Humanizer;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using TimeSpanParserUtil;

namespace BouncerBot.Modules.Achieve.Modules;

[ManageRolesSlashCommand(AchieveModuleMetadata.ModuleName, AchieveModuleMetadata.ModuleDescription)]
public class AchieveModule(
    IOptions<BouncerBotOptions> options,
    IAchievementService achievementService,
    IAchievementRoleOrchestrator achievementRoleOrchestrator,
    IRoleService roleService,
    IAchievementLockService achievementLockService) : ApplicationCommandModule<ApplicationCommandContext>
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
    public async Task ResetAchievementAsync(AchievementRole achievement,
        [SlashCommandParameter(Description = "Skip adding the Achiever role to users")] bool skipAchiever = false)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        try
        {
            var achievementRole = EnumUtils.ToRole(achievement);
            var numUsers = await roleService.GetRoleUserCount(Context.Guild!.Id, achievementRole);
            var numNewAchievers = await roleService.GetRoleUserCountWithExclude(Context.Guild.Id, achievementRole, exclude: Role.Achiever);

            await ModifyResponseAsync(m =>
            {
                m.Components = [
                    new ComponentContainerProperties()
                        .WithAccentColor(new Color(options.Value.Colors.Warning))
                        .AddComponents(
                            new TextDisplayProperties($"""
                                Are you sure you want to reset all the roles for {achievement.Humanize()}?

                                This will remove the role from {numUsers} users.
                                {(skipAchiever ? "I will skip adding Achiever roles." : $"Amount of new Achiever roles to bestow: {numNewAchievers}.")}
                                """
                            )
                        ),
                    new ActionRowProperties()
                        .AddComponents(new ButtonProperties($"achieve reset confirm:{(int)achievement}:{skipAchiever}", "Confirm", ButtonStyle.Danger))
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

    [SubSlashCommand(AchieveModuleMetadata.LockCommand.Name, AchieveModuleMetadata.LockCommand.Description)]
    public async Task LockAsync(
        [SlashCommandParameter(Description = "Duration (e.g. '2h', '30m', '1h30m')")]string duration)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        if (!TimeSpanParser.TryParse(duration, out var lockDuration))
        {
            await ModifyResponseAsync(m =>
            {
                new ComponentContainerProperties()
                    .WithAccentColor(new Color(options.Value.Colors.Error))
                    .AddTextDisplay("Invalid duration format. Please use formats like '2h', '30m', or '1h30m'.")
                    .Build(m);
            });
            return;
        }

        await achievementLockService.SetLockAsync(Context.Guild!.Id, lockDuration);

        await ModifyResponseAsync(m =>
        {
            new ComponentContainerProperties()
                .WithAccentColor(new Color(options.Value.Colors.Success))
                .AddTextDisplay($"""
                🔒 Achievement claims locked
                
                Duration: {lockDuration.Humanize()}
                Expires: <t:{DateTimeOffset.UtcNow.Add(lockDuration).ToUnixTimeSeconds()}:F>
                """)
                .Build(m);
        });
    }

    [SubSlashCommand(AchieveModuleMetadata.UnlockCommand.Name, AchieveModuleMetadata.UnlockCommand.Description)]
    public async Task UnlockAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        await achievementLockService.RemoveLockAsync(Context.Guild!.Id);
        await ModifyResponseAsync(m =>
        {
            new ComponentContainerProperties()
                .WithAccentColor(new Color(options.Value.Colors.Success))
                .AddTextDisplay("🔓 Achievement claims unlocked")
                .Build(m);
        });
    }
}
