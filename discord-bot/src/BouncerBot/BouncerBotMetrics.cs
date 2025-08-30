using System.Diagnostics;
using System.Diagnostics.Metrics;
using BouncerBot;

namespace BouncerBot;

public interface IBouncerBotMetrics
{
    void RecordCommand(string commandName);
}

internal sealed class BouncerBotMetrics : IBouncerBotMetrics, IDisposable
{
    private const string MeterName = "BouncerBot";

    // Tag key constants
    public static class TagKeys
    {
        public const string CommandName  = "command.name";
        public const string CommandModule = "command.module";
        public const string Outcome = "outcome";          // success|failure
        public const string GuildId = "guild.id";         // optional
    }

    private readonly Meter _meter;
    private readonly Counter<long> _commandInvocations;
    //private readonly Histogram<double> _commandDurationSeconds;

    public BouncerBotMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        _commandInvocations = _meter.CreateCounter<long>(
            name: "bouncerbot.slash_command.count",
            unit: "{invocations}",
            description: "Number of Discord slash command invocations");

        //_commandDurationSeconds = _meter.CreateHistogram<double>(
        //    name: "bouncerbot.slash_command.duration",
        //    unit: "s",
        //    description: "Execution duration of Discord slash commands in seconds");
    }

    public void RecordCommand(string commandName)
    {
        var tags = new TagList();
        AddCommandTag(ref tags, commandName);

        _commandInvocations.Add(1, tags);
    }

    public void Dispose()
    {
        _meter.Dispose();
    }

    private static void AddModuleTag(ref TagList tags, string? module)
    {
        if (module is not null)
            tags.Add("bouncerbot.command.module", module);
    }

    private static void AddCommandTag(ref TagList tags, string command)
    {
        if (command is not null)
            tags.Add("bouncerbot.command.name", command);
    }

    private static void AddErrorTag(ref TagList tags, Exception? exception)
    {
        var errorType = exception?.GetType().FullName;
        if (errorType is not null)
            tags.Add("error.type", errorType);
    }
}
