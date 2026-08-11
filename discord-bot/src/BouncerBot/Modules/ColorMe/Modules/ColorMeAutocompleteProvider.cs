using BouncerBot.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.ColorMe.Modules;

public sealed class ColorMeAutocompleteProvider(
    IDiscordGatewayClient gatewayClient,
    IColorRoleRegistry colorRoleRegistry,
    IRoleService roleService)
    : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
        ApplicationCommandInteractionDataOption option,
        AutocompleteInteractionContext context)
    {
        if (context.Guild is null)
        {
            return [];
        }

        var colorRoles = colorRoleRegistry.GetRoles(context.Guild.Id);
        var user = ColorMeHelpers.GetGuildUser(gatewayClient, context.Guild.Id, context.User.Id);
        var achievementRoleIds = await ColorMeHelpers.GetAchievementRoleIdsAsync(context.Guild.Id, colorRoles, roleService);
        var availableColors = ColorMeHelpers.GetAvailableColors(user, colorRoles, achievementRoleIds);
        var search = option.Value ?? string.Empty;

        return availableColors
            .Where(color => color.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(color => color.PowerType is not null)
            .Select(color => new ApplicationCommandOptionChoiceProperties(TrimLabel(color.Name), color.RoleId.ToString()))
            .Append(new ApplicationCommandOptionChoiceProperties("None", ColorMeModuleMetadata.RemoveColorValue))
            .Where(choice => choice.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Take(25)
            .ToArray();
    }

    private static string TrimLabel(string label)
        => label.Length <= 100 ? label : label[..100];
}
