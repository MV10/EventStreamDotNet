## Dependency Injected Services

The library fully supports dependency injection. Library services should be registered with the singleton scope. The library provides an `AddEventStreamDotNet` helper extension to make configuration and registration easier. This is the recommended configuration pattern. The extension method accepts an optional `ILoggerFactory` to enable [debug logging](services_logging.md), and optional configuration lambdas corresponding to each of the library services.

An example which references two domain models (based on the second example in the [Configuration Basics](configuration_basics.md) topic), domain event handlers for each, and projection handlers for one of the domain models:

```csharp
// not shown: AppConfig reads appsettings.json

services.AddEventStreamDotNet(
    loggerFactory: null, // no debug logging
    domainModelConfigs: cfg =>
    {
        // settings are instances of EventStreamDotNetConfig read from appsettings.json
        cfg.AddConfiguration<Customer>(AppConfig.Get.CustomerModelSettings);
        cfg.AddConfiguration<HumanResources>(AppConfig.Get.HumanResourcesModelSettings);
    },
    domainEventHandlers: cfg =>
    {
        cfg.RegisterDomainEventHandler<Customer, CustomerEventHandler>();
        cfg.RegisterDomainEventHandler<HumanResources, HumanResourcesEventHandler>();
    },
    projectionHandlers: cfg =>
    {
        cfg.RegisterProjectionHandler<Customer, CustomerProjectionHandler>();
    });
);
```

---

[Return to the Library Services topic](services.md)
