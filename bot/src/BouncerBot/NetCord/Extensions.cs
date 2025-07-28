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
}
