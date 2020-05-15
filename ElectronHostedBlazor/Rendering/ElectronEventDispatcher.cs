// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;

namespace ElectronHostedBlazor.Rendering
{
    /// <summary>
    /// Provides mechanisms for dispatching events to components in a <see cref="Renderer"/>.
    /// </summary>
    internal sealed class ElectronEventDispatcher
    {
        private readonly ElectronRenderer _renderer;

        public ElectronEventDispatcher(ElectronRenderer renderer)
        {
            _renderer = renderer;
        }

        public Task DispatchEvent(dynamic eventDescriptor, string eventArgsJson)
        {
            EventFieldInfo fieldInfo = null;
            using var sourceFieldInfo = eventDescriptor.eventFieldInfo;
            if (sourceFieldInfo != null)
            {
                fieldInfo = new EventFieldInfo
                {
                ComponentId = (int) sourceFieldInfo.componentId,
                FieldValue = sourceFieldInfo.fieldValue
                };
            }
            return DispatchEventOriginal(new WebEventDescriptor
                {
                    // TODO DM 26.08.2019: Replace those casts once we support proper number handling
                    BrowserRendererId = (int) eventDescriptor.browserRendererId,
                        EventHandlerId = (ulong) eventDescriptor.eventHandlerId,
                        EventArgsType = eventDescriptor.eventArgsType,
                        EventFieldInfo = fieldInfo
                },
                eventArgsJson);
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        //[JSInvokable(nameof(DispatchEvent))]
        private Task DispatchEventOriginal(WebEventDescriptor eventDescriptor, string eventArgsJson)
        {
            var webEvent = WebEventData.Parse(eventDescriptor, eventArgsJson);            
            return _renderer.DispatchEventAsync(
                webEvent.EventHandlerId,
                webEvent.EventFieldInfo,
                webEvent.EventArgs);
        }

        // Copied from Microsoft
        internal class WebEventData
        {
            // This class represents the second half of parsing incoming event data,
            // once the type of the eventArgs becomes known.
            public static WebEventData Parse(string eventDescriptorJson, string eventArgsJson)
            {
                WebEventDescriptor eventDescriptor;
                try
                {
                    eventDescriptor = Deserialize<WebEventDescriptor>(eventDescriptorJson);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Error parsing the event descriptor", e);
                }

                return Parse(
                    eventDescriptor,
                    eventArgsJson);
            }

            public static WebEventData Parse(WebEventDescriptor eventDescriptor, string eventArgsJson)
            {
                return new WebEventData(
                    eventDescriptor.BrowserRendererId,
                    eventDescriptor.EventHandlerId,
                    InterpretEventFieldInfo(eventDescriptor.EventFieldInfo),
                    ParseEventArgsJson(eventDescriptor.EventHandlerId, eventDescriptor.EventArgsType, eventArgsJson));
            }

            private WebEventData(int browserRendererId, ulong eventHandlerId, EventFieldInfo eventFieldInfo, EventArgs eventArgs)
            {
                BrowserRendererId = browserRendererId;
                EventHandlerId = eventHandlerId;
                EventFieldInfo = eventFieldInfo;
                EventArgs = eventArgs;
            }

            public int BrowserRendererId { get; }

            public ulong EventHandlerId { get; }

            public EventFieldInfo EventFieldInfo { get; }

            public EventArgs EventArgs { get; }

            private static EventArgs ParseEventArgsJson(ulong eventHandlerId, string eventArgsType, string eventArgsJson)
            {
                try
                {
                    switch (eventArgsType)
                    {
                        case "change":
                            return DeserializeChangeEventArgs(eventArgsJson);
                        case "clipboard":
                            return Deserialize<ClipboardEventArgs>(eventArgsJson);
                        case "drag":
                            return Deserialize<DragEventArgs>(eventArgsJson);
                        case "error":
                            return Deserialize<ErrorEventArgs>(eventArgsJson);
                        case "focus":
                            return Deserialize<FocusEventArgs>(eventArgsJson);
                        case "keyboard":
                            return Deserialize<KeyboardEventArgs>(eventArgsJson);
                        case "mouse":
                            return Deserialize<MouseEventArgs>(eventArgsJson);
                        case "pointer":
                            return Deserialize<PointerEventArgs>(eventArgsJson);
                        case "progress":
                            return Deserialize<ProgressEventArgs>(eventArgsJson);
                        case "touch":
                            return Deserialize<TouchEventArgs>(eventArgsJson);
                        case "unknown":
                            return EventArgs.Empty;
                        case "wheel":
                            return Deserialize<WheelEventArgs>(eventArgsJson);
                        default:
                            throw new InvalidOperationException($"Unsupported event type '{eventArgsType}'. EventId: '{eventHandlerId}'.");
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"There was an error parsing the event arguments. EventId: '{eventHandlerId}'.", e);
                }
            }

            private static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonSerializerOptionsProvider.Options);

            private static EventFieldInfo InterpretEventFieldInfo(EventFieldInfo fieldInfo)
            {
                // The incoming field value can be either a bool or a string, but since the .NET property
                // type is 'object', it will deserialize initially as a JsonElement
                if (fieldInfo?.FieldValue is JsonElement attributeValueJsonElement)
                {
                    switch (attributeValueJsonElement.ValueKind)
                    {
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            return new EventFieldInfo
                            {
                                ComponentId = fieldInfo.ComponentId,
                                    FieldValue = attributeValueJsonElement.GetBoolean()
                            };
                        default:
                            return new EventFieldInfo
                            {
                                ComponentId = fieldInfo.ComponentId,
                                    FieldValue = attributeValueJsonElement.GetString()
                            };
                    }
                }

                return null;
            }

            private static ChangeEventArgs DeserializeChangeEventArgs(string eventArgsJson)
            {
                var changeArgs = Deserialize<ChangeEventArgs>(eventArgsJson);
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
                        throw new ArgumentException($"Unsupported {nameof(ChangeEventArgs)} value {jsonElement}.");
                }
                return changeArgs;
            }
        }

        internal static class JsonSerializerOptionsProvider
        {
            public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
        }

    }
}