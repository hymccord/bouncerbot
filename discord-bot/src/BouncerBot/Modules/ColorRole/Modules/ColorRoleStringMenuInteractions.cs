using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace BouncerBot.Modules.ColorRole.Modules;

public class ColorRoleStringMenuInteractions(
    IOptions<BouncerBotOptions> options,
    IDiscordGatewayClient gatewayClient)
    : ComponentInteractionModule<StringMenuInteractionContext>
{
    [ComponentInteraction("color role select")]
    public async Task SelectColorAsync()
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        if (Context.Guild is null || Context.SelectedValues.Count != 1)
        {
            await ShowResultAsync("I could not determine which color to use.", options.Value.Colors.Error);
            return;
        }

        var guildId = Context.Guild.Id;
        var user = ColorRoleHelpers.GetGuildUser(gatewayClient, guildId, Context.User.Id);
        var colorOptions = options.Value.ColorRoles;
        var selectedValue = Context.SelectedValues[0];
        var managedRoleIds = ColorRoleHelpers.GetManagedRoleIds(colorOptions);

        try
        {
            if (selectedValue == ColorRoleModuleMetadata.RemoveColorValue)
            {
                foreach (var roleId in managedRoleIds.Where(user.RoleIds.Contains))
                {
                    await user.RemoveRoleAsync(roleId);
                }

                await ShowResultAsync("Your color role has been removed.", options.Value.Colors.Success);
                return;
            }

            // Re-check access at selection time. This prevents an old menu from granting a
            // fancy color after the user's access role has been removed.
            var availableColors = ColorRoleHelpers.GetAvailableColors(user, colorOptions);
            var selectedColor = availableColors.FirstOrDefault(color => color.RoleId.ToString() == selectedValue);

            if (selectedColor is null)
            {
                await ShowResultAsync("You no longer have access to that color.", options.Value.Colors.Warning);
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

            await ShowResultAsync($"Your color has been changed to **{selectedColor.Name}**.", options.Value.Colors.Success);
        }
        catch (Exception ex)
        {
            await ShowResultAsync(
                $"I could not update your color role. Please make sure BouncerBot's role is above all configured color roles.\n\nError: `{ex.Message}`",
                options.Value.Colors.Error);
        }
    }

    private async Task ShowResultAsync(string message, int color)
    {
        await ModifyResponseAsync(response =>
        {
            response.Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new(color))
                    .AddComponents(new TextDisplayProperties(message))
            ];
            response.Flags = MessageFlags.Ephemeral | MessageFlags.IsComponentsV2;
        });
    }
}
