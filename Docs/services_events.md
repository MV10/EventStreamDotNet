## "Domain Event Handlers" Service

The `DomainEventHandlersService` manages domain event handlers and their `Apply` methods. Like all of the library services, this data is keyed on a domain data model root.

You must add a domain event handler to this service before attempting to work with event streams for a given domain. 

The configurations service must have a valid configuration for the domain.

While it is recommended to configure the library services using the lambda configuration pattern (described in the [dependency injection](services_injected.md) and [non-injected](services_direct.md) topics), it is also possible to create and configure the domain event handlers in a more traditional line-by-line fashion using the `RegisterDomainEventHandler` method.

---

[Return to the Library Services topic](services.md)
