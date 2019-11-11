// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

export function getFileNameFromUrl(url: string) {
  // This could also be called "get last path segment from URL", but the primary
  // use case is to extract things that look like filenames
  const lastSegment = url.substring(url.lastIndexOf('/') + 1);
  const queryStringStartPos = lastSegment.indexOf('?');
  return queryStringStartPos < 0 ? lastSegment : lastSegment.substring(0, queryStringStartPos);
}

export function getAssemblyNameFromUrl(url: string) {
  return getFileNameFromUrl(url).replace(/\.dll$/, '');
}
