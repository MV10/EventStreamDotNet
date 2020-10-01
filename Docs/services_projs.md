## "Projection Handlers" Service

The `ProjectionHandlersService` manages projection handlers. Like all of the library services, this data is keyed on a domain data model root.

Projection handlers are optional, but the service must still be created (and registered for injection if your application uses DI) even if no projection handlers are used.

The configurations service must have a valid configuration for the domain.

While it is recommended to configure the library services using the lambda configuration pattern (described in the [dependency injection](services_injected.md) and [non-injected](services_direct.md) topics), it is also possible to create and configure the projection handlers in a more traditional line-by-line fashion using the `RegisterProjectionHandler` method.

---

[Return to the Library Services topic](services.md)
