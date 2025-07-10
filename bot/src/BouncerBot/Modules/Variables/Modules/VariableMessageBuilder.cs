using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetCord.Rest;
using NetCord;

namespace BouncerBot.Modules.Variables.Modules;
internal static class VariableMessageBuilder
{
    public static InteractionMessageProperties CreateChannelMenuMessage(string channel, ulong? defaultValue = null) => new()
    {
        Embeds = [
                new EmbedProperties() {
                    Title = "Select a channel setting to change"
                }
            ],
        Components = [
               new ActionRowProperties().WithButtons([
                   new ButtonProperties("variables channels:log:", "", ButtonStyle.Secondary),
                   new ButtonProperties("variables channels:achievement:", "", ButtonStyle.Secondary),
                   new ButtonProperties("variables channels:eggmaster:", "", ButtonStyle.Secondary),
                   new ButtonProperties("variables channels:verification:", "", ButtonStyle.Secondary)
               ]),
            ],
        Flags = MessageFlags.Ephemeral
    };
}
