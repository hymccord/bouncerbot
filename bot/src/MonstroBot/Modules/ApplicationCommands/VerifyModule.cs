using MonstroBot.Attributes;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace MonstroBot.Modules.ApplicationCommands;

[SlashCommand("verify", "Manage MouseHunt ID verification")]
[GuildOnly<SlashCommandContext>]
public class VerifyModule : ApplicationCommandModule<SlashCommandContext>
{
    [SubSlashCommand("me", "Verify you own a MouseHunt ID")]
    public InteractionMessageProperties VerifyMe(
        [SlashCommandParameter(MinValue = 1)] uint mouseHuntId
    )
    {
        throw new NotImplementedException();

        //return new MessageProperties
        //{
        //    Content = $"You have been verified with MouseHunt ID: {mouseHuntId}",
        //    Flags = MessageFlags.Ephemeral
        //};
        // Here you would typically check the database to see if the user is already verified
        // and then add them if they are not.
        //await context.RespondAsync($"You have been verified with MouseHunt ID: {mouseHuntId}");
    }

    [SubSlashCommand("remove", "Remove your MouseHunt ID verification")]
    [ManageMessageOnly<SlashCommandContext>]
    public InteractionMessageProperties RemoveVerification()
    {
        return new InteractionMessageProperties()
            .WithFlags(MessageFlags.Ephemeral)
            .WithContent("A secret Hello!");
        // Here you would typically remove the user's verification from the database.

        return "Your MouseHunt ID verification has been removed.";
        //await context.RespondAsync("Your MouseHunt ID verification has been removed.");
    }
}
