// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Rendering
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.RenderTree;
    using Microsoft.AspNetCore.Components.Web;

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
                fieldInfo = new EventFieldInfo { ComponentId = (int)sourceFieldInfo.componentId, FieldValue = sourceFieldInfo.fieldValue };
            }

            return DispatchEventOriginal(new WebEventDescriptor
                                         {
                                             // TODO DM 26.08.2019: Replace those casts once we support proper number handling
                                             BrowserRendererId = (int)eventDescriptor.browserRendererId,
                                             EventHandlerId = (ulong)eventDescriptor.eventHandlerId,
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
    }
}
