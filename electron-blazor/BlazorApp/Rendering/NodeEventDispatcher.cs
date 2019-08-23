using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorApp.Rendering
{
    /// <summary>
    /// Dispatches events from JavaScript to a Blazor Node renderer.
    /// Intended for internal use only.
    /// </summary>
    public static class NodeEventDispatcher
    {
        /*
        /// <summary>
        /// For framework use only.
        /// </summary>
        [JSInvokable(nameof(DispatchEvent))]
        public static Task DispatchEvent(WebEventDescriptor eventDescriptor, string eventArgsJson)
        {
            var webEvent = WebEventData.Parse(eventDescriptor, eventArgsJson);
            var renderer = RendererRegistry.Find(eventDescriptor.BrowserRendererId);
            return renderer.DispatchEventAsync(
                webEvent.EventHandlerId,
                webEvent.EventFieldInfo,
                webEvent.EventArgs);
        }
        */
    }
}