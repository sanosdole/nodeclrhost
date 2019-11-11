// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

/* eslint-disable no-console */

import { Logger, LogLevel } from './Logger';

export class NullLogger implements Logger {
  public static instance: Logger = new NullLogger();

  private constructor() { }

  public log(_logLevel: LogLevel, _message: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars
  }
}

export class ConsoleLogger implements Logger {
  private readonly minimumLogLevel: LogLevel;

  public constructor(minimumLogLevel: LogLevel) {
    this.minimumLogLevel = minimumLogLevel;
  }

  public log(logLevel: LogLevel, message: string | Error): void {
    if (logLevel >= this.minimumLogLevel) {
      switch (logLevel) {
        case LogLevel.Critical:
        case LogLevel.Error:
          console.error(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
          break;
        case LogLevel.Warning:
          console.warn(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
          break;
        case LogLevel.Information:
          console.info(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
          break;
        default:
          // console.debug only goes to attached debuggers in Node, so we use console.log for Trace and Debug
          console.log(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
          break;
      }
    }
  }
}
