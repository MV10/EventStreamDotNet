## Configuring Debug Logging

The library can output debug logging if the client application provides a standard `ILoggerFactory` interface. This should only be activated when required as it can generate a large amount of log data in a busy application.

The demo project shows how to wire up Serilog for console-based logger output, although any _`Microsoft.Extensions.Logging`_-compatible `ILoggerFactory` should work. (Most other logging packages such as NLog now support this model.)

---

#### `LoggerFactory`

Simply set this to an `ILoggerFactory` and the library will do the rest.

---

[Return to the Configuration topic](configuration.md)
