using NetCord.Gateway;
using NetCord.Rest;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NetCord;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class NetCordExtensions
{
    extension(InteractionCallback)
    {
        /// <inheritdoc cref="InteractionCallback.DeferredMessage(MessageFlags?)"/>
        public static InteractionCallbackProperties<InteractionMessageProperties> DeferredEphemeralMessage()
            => InteractionCallback.DeferredMessage(MessageFlags.Ephemeral);
    }

    extension(PartialGuildUser partialGuildUser)
    {
        public Permissions GetResolvedChannelPermissions(Guild guild, ulong channelId)
        {
            var permissions = partialGuildUser.GetChannelPermissions(guild, channelId);

            if (!permissions.HasFlag(Permissions.ViewChannel))
            {
                permissions = 0;
            }
            else if (!permissions.HasFlag(Permissions.SendMessages))
            {
                permissions &= ~Permissions.SendTtsMessages;
                permissions &= ~Permissions.MentionEveryone;
                permissions &= ~Permissions.EmbedLinks;
                permissions &= ~Permissions.AttachFiles;
            }

            return permissions;
        }
    }
}
