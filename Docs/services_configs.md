## "Domain Model Configurations" Service

The `EventStreamConfigService` manages sets of configuration data (meaning the various settings which are typically read from a source like `appsettings.json`, as discussed in the [Configuration](configuration.md) topics). Like all of the library services, this data is keyed on a domain data model root.

You must configure this library service before the others. The other services read configuration from this one.

You must add at least one configuration to this service before attempting to work with event streams. 

While it is recommended to configure the library services using the lambda configuration pattern (described in the [dependency injection](services_injected.md) and [non-injected](services_direct.md) topics), it is also possible to create and configure the services in a more traditional line-by-line fashion using the `AddConfiguration` method.

---

[Return to the Library Services topic](services.md)
