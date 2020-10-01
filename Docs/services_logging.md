## Configuring Debug Logging

The library can output debug logging if the client application provides a standard `ILoggerFactory` object. This should only be activated when debugging is actually required as it can generate a large amount of log data in a busy application.

The library's `EventStreamConfigService` object exposes an `ILoggerFactory` property called `LoggerFactory`, and the recommended lambda-style service configuration helpers each accept an optional `loggerFactory` argument (refer to the [dependency injection](services_injected.md) and [non-injected](services_direct.md) topics).

The demo project shows how to wire up Serilog for console-based logger output, although any _`Microsoft.Extensions.Logging`_-compatible `ILoggerFactory` should work. (Most other logging packages such as NLog now support this model.)

---

[Return to the Library Services topic](services.md)
