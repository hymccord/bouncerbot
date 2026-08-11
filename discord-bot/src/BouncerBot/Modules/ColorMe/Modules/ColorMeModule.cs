using BouncerBot.Attributes;
using BouncerBot.Services;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.ColorMe.Modules;

public class ColorMeModule(
    IOptions<BouncerBotOptions> options,
    IDiscordGatewayClient gatewayClient,
    IColorRoleRegistry colorRoleRegistry,
    IRoleService roleService,
    IBouncerBotMetrics bouncerBotMetrics
) : ApplicationCommandModule<ApplicationCommandContext>
{
    [BouncerBotSlashCommand(ColorMeModuleMetadata.ColorCommand.Name, ColorMeModuleMetadata.ColorCommand.Description)]
    public async Task ColorAsync(
        [SlashCommandParameter(
            Name = "color",
            Description = "The color to use, or 'None' to clear your current color.",
            AutocompleteProviderType = typeof(ColorMeAutocompleteProvider))]
        string color)
    {
        bouncerBotMetrics.RecordCommand(ColorMeModuleMetadata.ColorCommand.Name);
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        if (Context.Guild is null)
        {
            await ShowMessageAsync("This command can only be used in a server.", options.Value.Colors.Error);
            return;
        }

        var user = ColorMeHelpers.GetGuildUser(gatewayClient, Context.Guild.Id, Context.User.Id);
        var colorRoles = colorRoleRegistry.GetRoles(Context.Guild.Id);
        var managedRoleIds = ColorMeHelpers.GetManagedRoleIds(colorRoles);

        try
        {
            if (color == ColorMeModuleMetadata.RemoveColorValue)
            {
                foreach (var roleId in managedRoleIds.Where(user.RoleIds.Contains))
                {
                    await user.RemoveRoleAsync(roleId);
                }

                await ShowMessageAsync("Your color role has been removed.", options.Value.Colors.Success);
                return;
            }

            // Re-check access at execution time so stale autocomplete results cannot bypass a role change.
            var achievementRoleIds = await ColorMeHelpers.GetAchievementRoleIdsAsync(Context.Guild.Id, colorRoles, roleService);
            var availableColors = ColorMeHelpers.GetAvailableColors(user, colorRoles, achievementRoleIds);
            var selectedColor = availableColors.FirstOrDefault(availableColor => availableColor.RoleId.ToString() == color);

            if (selectedColor is null)
            {
                await ShowMessageAsync("Your color selection is invalid.", options.Value.Colors.Warning);
                return;
            }

            // Add the requested role first so a transient failure does not remove the user's
            // existing color before the new one can be assigned.
            if (!user.RoleIds.Contains(selectedColor.RoleId))
            {
                await user.AddRoleAsync(selectedColor.RoleId);
            }

            foreach (var roleId in managedRoleIds.Where(roleId => roleId != selectedColor.RoleId && user.RoleIds.Contains(roleId)))
            {
                await user.RemoveRoleAsync(roleId);
            }

            await ShowMessageAsync($"Your color has been changed to **{selectedColor.Name}**.", options.Value.Colors.Success);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                $"I could not update your color role. Please make sure BouncerBot's role is above all configured color roles.\n\nError: `{ex.Message}`",
                options.Value.Colors.Error);
        }
    }

    private async Task ShowMessageAsync(string message, int color)
    {
        await ModifyResponseAsync(response =>
        {
            response.Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new(color))
                    .AddComponents(new TextDisplayProperties(message))
            ];
            response.Flags = MessageFlags.IsComponentsV2 | MessageFlags.Ephemeral;
        });
    }
}
