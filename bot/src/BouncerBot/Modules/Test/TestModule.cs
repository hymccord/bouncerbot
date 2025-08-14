using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Test;

#if DEBUG
public class TestModule(
    IOptionsSnapshot<BouncerBotOptions> options
) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("test", "Test description")]
    public async Task TestAsync()
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Components = [
                new ComponentContainerProperties()
                    .WithAccentColor(new (options.Value.Colors.Primary))
                    .WithComponents([
                        new TextDisplayProperties("Primary!")
                        ]),
                new ComponentContainerProperties()
                    .WithAccentColor(new (options.Value.Colors.Success))
                    .WithComponents([
                        new TextDisplayProperties("Success!")
                        ]),
                new ComponentContainerProperties()
                    .WithAccentColor(new (options.Value.Colors.Warning))
                    .WithComponents([
                        new TextDisplayProperties("Warning!")
                        ]),
                new ComponentContainerProperties()
                    .WithAccentColor(new (options.Value.Colors.Error))
                    .WithComponents([
                        new TextDisplayProperties("Error!")
                        ])
            ],
            Flags = MessageFlags.IsComponentsV2
        }));
    }
}
#endif
