## Projection Support

Projections are optional handlers which extract data from your domain model data (if you are not familiar with these concepts, please review the [Architectural Patterns](patterns.md) page first). The library addresses projection configuration, declaring projection trigger conditions, and registration of projection handlers.

### Projection Configuration

The library configuration system will pass the projection settings to projection handler constructors. Refer to the [configuration documentation](configuration.md) for more information about creating and reading config settings.

### Projection Handler Classes

A projection handler class must inherit from the library's `IDomainModelProjectionHandler<TDomainModelRoot>` interface. In addition to a constructor that accepts a `ProjectionConfig` object argument, the interface requires implementing a `DomainModelState` property of the domain model root class type. The next section describes how to add projection methods to the class.

Projection handlers must be registered at application startup (see the [Projection Handlers Service](services_projs.md) topic for more information).

### Projection Handler Methods

Projection methods are required to be `async Task` with no arguments. Before projections are invoked, the domain model object's current state will be copied into the handler's `DomainModelState` property to use as a reference for the projection. `DomainModelState` is transient and must not be used for any other purpose. After the projections have been invoked, the manager will reset the property to null.

Projections can be invoked either as a result of a snapshot update, or in response to one or more domain events. The methods are marked with any combination of a `[SnapshotProjection]` and one or more `[DomainEventProjection(Type)]` attributes.

Domain event projections are invoked in the order in which the events occurred, followed by any snapshot projections. If a batch of domain events is processed, the projections are invoked after all events are stored and applied, and after any resuting snapshot update is created. If a domain event occurs more than once in a batch, any related projections are invoked only once (they operate on the same single end-result domain model object, so multiple invocations would be pointless).

Generally you should prefer domain event projections over snapshot projections, except for very simple domain models. Snapshots may happen frequently based on changes unrelated to the data extracted by a projection. Domain events are more targeted.

You should carefully null-check the domain model data, particularly with snapshot projections. (Consider that the initial `StreamInitialized` domain event is just a `new()` on the domain model root, followed by a snapshot, so the domain model object is still probably mostly invalid.)

These examples are from the demo project:

```csharp
[SnapshotProjection]
public async Task ProjectCustomerResidency()
{ ... }

[DomainEventProjection(typeof(SpouseChanged))]
[DomainEventProjection(typeof(SpouseRemoved))]
public async Task ProjectCustomerMaritalStatus()
{ ... }
```

---

[Return to the documentation index](index.md)
