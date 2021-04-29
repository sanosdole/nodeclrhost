// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

import { EventDescriptor } from './BrowserRenderer';
import { UIEventArgs } from './EventForDotNet';

type EventDispatcher = (eventDescriptor: EventDescriptor, eventArgs: UIEventArgs) => void;

let eventDispatcherInstance: EventDispatcher;

export function dispatchEvent(eventDescriptor: EventDescriptor, eventArgs: UIEventArgs): void {
  if (!eventDispatcherInstance) {
    throw new Error('eventDispatcher not initialized. Call \'setEventDispatcher\' to configure it.');
  }

  eventDispatcherInstance(eventDescriptor, eventArgs);
}

export function setEventDispatcher(newDispatcher: (eventDescriptor: EventDescriptor, eventArgs: UIEventArgs) => void): void {
  eventDispatcherInstance = newDispatcher;
}
