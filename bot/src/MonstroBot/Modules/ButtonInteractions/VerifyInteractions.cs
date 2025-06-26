using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MonstroBot.Db;

using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace MonstroBot.Modules.ButtonInteractions;

public class VerifyInteractions(IDbContextFactory<MonstroBotDbContext> dbContextFactory) : ComponentInteractionModule<ComponentInteractionContext>
{
    [ComponentInteraction("verify:{id}")]
    public async Task VerifyMe(int mhid)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(), true);
    }
}
