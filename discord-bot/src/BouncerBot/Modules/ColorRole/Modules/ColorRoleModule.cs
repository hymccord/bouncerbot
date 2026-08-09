using BouncerBot.Attributes;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.ColorRole.Modules;

public class ColorRoleModule(
    IOptions<BouncerBotOptions> options,
    IDiscordGatewayClient gatewayClient,
    IBouncerBotMetrics bouncerBotMetrics
) : ApplicationCommandModule<ApplicationCommandContext>
{
    private const int MaxColors = 25;

    [BouncerBotSlashCommand(ColorRoleModuleMetadata.ColorCommand.Name, ColorRoleModuleMetadata.ColorCommand.Description)]
    public async Task ColorAsync()
    {
        bouncerBotMetrics.RecordCommand(ColorRoleModuleMetadata.ColorCommand.Name);
        await RespondAsync(InteractionCallback.DeferredEphemeralMessage());

        if (Context.Guild is null)
        {
            await ShowMessageAsync("This command can only be used in a server.", options.Value.Colors.Error);
            return;
        }

        var colorOptions = options.Value.ColorRoles;
        var user = ColorRoleHelpers.GetGuildUser(gatewayClient, Context.Guild.Id, Context.User.Id);
        var availableColors = ColorRoleHelpers.GetAvailableColors(user, colorOptions);

        if (availableColors.Count == 0)
        {
            await ShowMessageAsync("You do not currently have a role that grants access to server colors.", options.Value.Colors.Warning);
            return;
        }

        // One menu slot is reserved for the "Remove color" option.
        if (availableColors.Count > MaxColors - 1)
        {
            await ShowMessageAsync(
                $"Color roles are misconfigured: {availableColors.Count} colors are available, but this menu only has room for {MaxColors - 1}. Please contact a server administrator.",
                options.Value.Colors.Error);
            return;
        }

        var selectOptions = availableColors
            .Select(color => new StringMenuSelectOptionProperties(TrimLabel(color.Name), color.RoleId.ToString()))
            .Append(new StringMenuSelectOptionProperties("Remove color", ColorRoleModuleMetadata.RemoveColorValue))
            .ToArray();

        var menu = new StringMenuProperties("color role select", selectOptions)
            .WithPlaceholder("Choose a color")
            .WithMinValues(1)
            .WithMaxValues(1);

        var container = new ComponentContainerProperties()
            .WithAccentColor(new(options.Value.Colors.Primary))
            .AddComponents(
                new TextDisplayProperties("### Choose your color\nSelect a color you have access to, or **Remove color** to clear your current one."),
                menu
            );

        await ModifyResponseAsync(message =>
        {
            message.Components = [container];
            message.Flags = MessageFlags.IsComponentsV2 | MessageFlags.Ephemeral;
        });
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

    private static string TrimLabel(string label)
        => label.Length <= 100 ? label : label[..100];
}
