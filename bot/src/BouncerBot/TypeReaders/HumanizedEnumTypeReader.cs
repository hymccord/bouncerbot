using System.Globalization;
using Humanizer;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.TypeReaders;
internal class HumanizedEnumTypeReader<T> : SlashCommandTypeReader<ApplicationCommandContext>
    where T : struct, Enum
{
    public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.Integer;

    public override ValueTask<TypeReaderResult> ReadAsync(string value, ApplicationCommandContext context, SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration, IServiceProvider? serviceProvider)
    {
        return ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result)
            ? new(TypeReaderResult.Success((T)Enum.ToObject(typeof(T), result)))
            : new(TypeReaderResult.ParseFail(parameter.Name));
    }

    public override Type? AutocompleteProviderType => typeof(HumanizedEnumAutocompleteProvider<T>);

    public override double? GetMinValue(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration) => 0;
}

internal class HumanizedEnumAutocompleteProvider<T> : IAutocompleteProvider<AutocompleteInteractionContext>
    where T : struct, Enum
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        // First is the enum name, second is the humanized version (sometimes an emoji)
        return new(
            Enum.GetValues<T>()
            .Select(v => new
            {
                Value = v,
                Humanized = v.Humanize(),
                Name = Enum.GetName(v)!
            })
            .Where(p => p.Name.Contains(option.Value!, StringComparison.InvariantCultureIgnoreCase) || p.Humanized.Contains(option.Value!, StringComparison.InvariantCultureIgnoreCase))
            .Select(p => new ApplicationCommandOptionChoiceProperties(p.Humanized, Convert.ToDouble(p.Value))).Take(25));
    }
}
