## Non-Injected Service Usage

The library can be used without dependency injection. The library provides an `DirectDependencyServiceHost` helper class to make configuration and registration easier. This is the recommended configuration pattern. The helper's constructor accepts an optional `ILoggerFactory` to enable [debug logging](services_logging.md), and optional configuration lambdas corresponding to each of the library services.

You would then pass a reference to this helper into the constructor of any `EventStreamCollection` or `EventStreamManager` objects your application needs.

An example which references two domain models (based on the second example in the [Configuration Basics](configuration_basics.md) topic), domain event handlers for each, and projection handlers for one of the domain models:

```csharp
// not shown: AppConfig reads appsettings.json

var eventLibraryServices = new DirectDependencyServiceHost(
    loggerFactory: null, // no debug logging
    domainModelConfigs: cfg =>
    {
        cfg.AddConfiguration<Customer>(AppConfig.Get.CustomerEventStream);
        cfg.AddConfiguration<HumanResources>(AppConfig.Get.HumanResourcesEventStream);
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

var customers = new EventStreamCollection<Customer>(eventLibraryServices);
```

---

[Return to the Library Services topic](services.md)
