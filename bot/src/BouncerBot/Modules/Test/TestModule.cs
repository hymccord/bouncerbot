using System.Globalization;
using Humanizer;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Test;

#if DEBUG
public class TestModule(
    IOptionsSnapshot<BouncerBotOptions> options
) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("test", "Test description")]
    public Task TestAsync([SlashCommandParameter(AutocompleteProviderType = typeof(FruitsAutocompleteProvider))] Fruits fruit)
    {
        options.Value.Emojis.Hydro = fruit.ToString();

        return Task.CompletedTask;
    }
}

internal class FruitsTypeReader : SlashCommandTypeReader<ApplicationCommandContext>
{
    public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.Integer;

    public override ValueTask<TypeReaderResult> ReadAsync(string value, ApplicationCommandContext context, SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration, IServiceProvider? serviceProvider)
    {
        return ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result)
            ? new(TypeReaderResult.Success((Fruits)result))
            : new(TypeReaderResult.ParseFail(parameter.Name));
    }

    public override Type? AutocompleteProviderType => typeof(FruitsAutocompleteProvider);

    public override double? GetMinValue(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration) => 0;
}

internal class FruitsAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return new(
            Enum.GetValues<Fruits>().Select(v => v.Humanize()).Where(p => p.Contains(option.Value!)).Select(p => new ApplicationCommandOptionChoiceProperties(p, (double)Enum.Parse<Fruits>(p))).Take(25));
    }
}

public enum Fruits
{
    Apple,
    Banana,
    Cherry,
    Date,
    Elderberry,
    Fig,
    Grape,
    Honeydew,
    ItaPalm,
    Jackfruit,
    Kiwi,
    Lemon,
    Mango,
    Nectarine,
    Orange,
    Papaya,
    Quince,
    Raspberry,
    Strawberry,
    Tangerine,
    UgliFruit,
    Voavanga,
    Watermelon,
    Xigua,
    YellowPassionFruit,
}

#endif
