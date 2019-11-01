// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

// Expose an export called 'platform' of the interface type 'Platform',
// so that consumers can be agnostic about which implementation they use.
// Basic alternative to having an actual DI container.
import { Platform } from './Platform/Platform';

export let platform: Platform;

export function setPlatform(platformInstance: Platform) {
  platform = platformInstance;
  return platform;
}
