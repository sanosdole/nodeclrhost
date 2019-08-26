using System;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorApp.Rendering
{
    /// <summary>
    /// Provides mechanisms for dispatching events to components in a <see cref="Renderer"/>.
    /// </summary>
    internal static class RendererRegistryEventDispatcher
    {
        internal static void Register(NodeJSRuntime runtime)
        {
            // TODO DM 26.08.2019: Return task once they are marshalled as promises
            runtime.Host.Global.window.Blazor._internal.HandleRendererEvent = new Action<dynamic, string>(DispatchEvent);
        }

        private static void DispatchEvent(dynamic eventDescriptor, string eventArgsJson)
        {
            // TODO DM 26.08.2019: This fails due to errors on string marshalling (missing NULL terminator?)
            EventFieldInfo fieldInfo = null;
            var sourceFieldInfo = eventDescriptor.eventFieldInfo;
            if (sourceFieldInfo != null)
            {
                fieldInfo = new EventFieldInfo
                {
                    ComponentId = eventDescriptor.eventFieldInfo.componentId,
                    FieldValue = eventDescriptor.eventFieldInfo.fieldValue
                };
            }
            DispatchEventOriginal(new BrowserEventDescriptor
                {
                    BrowserRendererId = (int) (double) eventDescriptor.browserRendererId,
                        EventHandlerId = (ulong) (double) eventDescriptor.eventHandlerId,
                        EventArgsType = eventDescriptor.eventArgsType,
                        EventFieldInfo = fieldInfo
                },
                eventArgsJson);
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        //[JSInvokable(nameof(DispatchEvent))]
        public static Task DispatchEventOriginal(BrowserEventDescriptor eventDescriptor, string eventArgsJson)
        {
            InterpretEventDescriptor(eventDescriptor);
            var eventArgs = ParseEventArgsJson(eventDescriptor.EventArgsType, eventArgsJson);
            var renderer = RendererRegistry.Find(eventDescriptor.BrowserRendererId);
            return renderer.DispatchEventAsync(eventDescriptor.EventHandlerId, eventDescriptor.EventFieldInfo, eventArgs);
        }

        private static void InterpretEventDescriptor(BrowserEventDescriptor eventDescriptor)
        {
            // The incoming field value can be either a bool or a string, but since the .NET property
            // type is 'object', it will deserialize initially as a JsonElement
            var fieldInfo = eventDescriptor.EventFieldInfo;
            if (fieldInfo != null)
            {
                if (fieldInfo.FieldValue is JsonElement attributeValueJsonElement)
                {
                    switch (attributeValueJsonElement.ValueKind)
                    {
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            fieldInfo.FieldValue = attributeValueJsonElement.GetBoolean();
                            break;
                        default:
                            fieldInfo.FieldValue = attributeValueJsonElement.GetString();
                            break;
                    }
                }
                else
                {
                    // Unanticipated value type. Ensure we don't do anything with it.
                    eventDescriptor.EventFieldInfo = null;
                }
            }
        }

        private static UIEventArgs ParseEventArgsJson(string eventArgsType, string eventArgsJson)
        {
            switch (eventArgsType)
            {
                case "change":
                    return DeserializeUIEventChangeArgs(eventArgsJson);
                case "clipboard":
                    return Deserialize<UIClipboardEventArgs>(eventArgsJson);
                case "drag":
                    return Deserialize<UIDragEventArgs>(eventArgsJson);
                case "error":
                    return Deserialize<UIErrorEventArgs>(eventArgsJson);
                case "focus":
                    return Deserialize<UIFocusEventArgs>(eventArgsJson);
                case "keyboard":
                    return Deserialize<UIKeyboardEventArgs>(eventArgsJson);
                case "mouse":
                    return Deserialize<UIMouseEventArgs>(eventArgsJson);
                case "pointer":
                    return Deserialize<UIPointerEventArgs>(eventArgsJson);
                case "progress":
                    return Deserialize<UIProgressEventArgs>(eventArgsJson);
                case "touch":
                    return Deserialize<UITouchEventArgs>(eventArgsJson);
                case "unknown":
                    return Deserialize<UIEventArgs>(eventArgsJson);
                case "wheel":
                    return Deserialize<UIWheelEventArgs>(eventArgsJson);
                default:
                    throw new ArgumentException($"Unsupported value '{eventArgsType}'.", nameof(eventArgsType));
            }
        }

        private static T Deserialize<T>(string eventArgsJson)
        {
            return JsonSerializer.Deserialize<T>(eventArgsJson /*, JsonSerializerOptionsProvider.Options*/ );
        }

        private static UIChangeEventArgs DeserializeUIEventChangeArgs(string eventArgsJson)
        {
            var changeArgs = Deserialize<UIChangeEventArgs>(eventArgsJson);
            var jsonElement = (JsonElement) changeArgs.Value;
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Null:
                    changeArgs.Value = null;
                    break;
                case JsonValueKind.String:
                    changeArgs.Value = jsonElement.GetString();
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    changeArgs.Value = jsonElement.GetBoolean();
                    break;
                default:
                    throw new ArgumentException($"Unsupported {nameof(UIChangeEventArgs)} value {jsonElement}.");
            }
            return changeArgs;
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        public class BrowserEventDescriptor
        {
            /// <summary>
            /// For framework use only.
            /// </summary>
            public int BrowserRendererId { get; set; }

            /// <summary>
            /// For framework use only.
            /// </summary>
            public ulong EventHandlerId { get; set; }

            /// <summary>
            /// For framework use only.
            /// </summary>
            public string EventArgsType { get; set; }

            /// <summary>
            /// For framework use only.
            /// </summary>
            public EventFieldInfo EventFieldInfo { get; set; }
        }
    }
}