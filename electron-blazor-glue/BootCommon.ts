// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

// Tells you if the script was added without <script src="..." autostart="false"></script>
export function shouldAutoStart() {
  return !!(document &&
    document.currentScript &&
    document.currentScript.getAttribute('autostart') !== 'false');
}
