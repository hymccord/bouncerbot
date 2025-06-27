using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MonstroBot.Db;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ComponentInteractions;

namespace MonstroBot.Modules.ButtonInteractions;

public class VerifyInteractions(ILogger<VerifyInteractions> logger, IDbContextFactory<MonstroBotDbContext> dbContextFactory) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("verifyme")]
    public async Task VerifyMe()
    {
        logger.LogDebug("Context Token: {InteractionToken}", Context.Interaction.Token);
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        //await DeleteResponseAsync();

        await Context.Interaction.Message.DeleteAsync();
        //await ModifyResponseAsync(x =>
        //{
        //    x.Content = "I sent a direct message with the details!";
        //    x.Flags = MessageFlags.Ephemeral;
        //});

        //var channel = await Context.Client.Rest.GetDMChannelAsync(Context.User.Id);

        //await channel.SendMessageAsync(new MessageProperties()
        //{
        //    Content = "I'm MonstroBot",
        //    Components = [
        //        new ActionRowProperties([
        //            new ButtonProperties("verifyme start", "Start", ButtonStyle.Success),
        //            new ButtonProperties("verifyme cancel", "Cancel", ButtonStyle.Danger)
        //            ])
        //        ]
        //});
    }
}
