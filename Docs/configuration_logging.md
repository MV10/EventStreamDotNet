## Configuring Debug Logging

The library can output debug logging if the client application provides a standard `ILoggerFactory` object. This should only be activated when debugging is actually required as it can generate a large amount of log data in a busy application.

The library's `EventStreamConfigService` object exposes an `ILoggerFactory` property called `LoggerFactory`. If you're using dependency injection (DI), pass your logger factory to the service class constructor. If you're not using DI, pass the logger factory to the constructor of the `DirectDependencyServiceHost` helper class instead.

The demo project shows how to wire up Serilog for console-based logger output, although any _`Microsoft.Extensions.Logging`_-compatible `ILoggerFactory` should work. (Most other logging packages such as NLog now support this model.)

---

[Return to the Configuration topic](configuration.md)
