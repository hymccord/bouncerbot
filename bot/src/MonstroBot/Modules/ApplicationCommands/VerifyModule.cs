using System.Threading.Tasks;

using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MonstroBot.Attributes;
using MonstroBot.Db;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace MonstroBot.Modules.ApplicationCommands;

[SlashCommand("verify", "Manage MouseHunt ID verification")]
[GuildOnly<ApplicationCommandContext>]
public class VerifyModule(ILogger<VerifyModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("me", "Verify you own a MouseHunt ID")]
    [RequireVerificationStatus<ApplicationCommandContext>(VerificationStatus.Unverified)]
    public async Task VerifyMe(
        [SlashCommandParameter(Description = "MouseHunt Profile ID", MinValue = 1)] uint mouseHuntId
    )
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        await ModifyResponseAsync(x =>
        {
            x.Content = "Click below to start the verification process!";
            x.AddComponents(
                new ActionRowProperties()
                    .AddButtons(new ButtonProperties($"verifyme", "Start!", ButtonStyle.Success))
                );
        });

        logger.LogDebug("The interaction token is {InteractionToken}", Context.Interaction.Token);

        //return new MessageProperties
        //{
        //    Content = $"You have been verified with MouseHunt ID: {mouseHuntId}",
        //    Flags = MessageFlags.Ephemeral
        //};
        // Here you would typically check the database to see if the user is already verified
        // and then add them if they are not.
        //await context.RespondAsync($"You have been verified with MouseHunt ID: {mouseHuntId}");
    }

    [SubSlashCommand("remove", "Remove a MouseHunt ID verification")]
    [ManageMessageOnly<ApplicationCommandContext>]
    public async Task<InteractionMessageProperties> RemoveVerification(
        [SlashCommandParameter(Description = "Discord User")] User user
        )
    {

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithFlags(MessageFlags.Ephemeral)
            .WithContent("A secret Hello!")
        ));
        
        // Here you would typically remove the user's verification from the database.

        return "Your MouseHunt ID verification has been removed.";
        //await context.RespondAsync("Your MouseHunt ID verification has been removed.");
    }
}
