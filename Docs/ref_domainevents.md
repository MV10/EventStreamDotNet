## Domain Event Support

Domain events track changes to your domain data model (if you are not familiar with these concepts, please review the [Architectural Patterns](patterns.md) page first). The library addresses two aspects relating to domain events -- declaring the events themselves, and handling the events.

### Domain Event Classes

A domain event is just a simple data-only class (POCO) which inherits from the library's `DomainEventBase` abstract class. The properties of the class should represent only the data which changed. The base class does not require implementing anything in the inheriting class. It just adds a few properties that associates the event with the model instance (the unique `Id` value), the version of the model (called an `ETag`), and a timestamp.

### Domain Event Handler Classes

Each domain data model needs a domain event handler class which implements the `IDomainEventHandler<TDomainModelRoot>` interface. The interface requires a `DomainModelState` property of the domain model root type, and a `void Apply` method which accepts a `StreamInitialized` domain event argument (although normally the event handler doesn't do anything in response to that event).

From there, you should add a `void Apply` method for each and every domain event defined for your model.

Before an `Apply` method is invoked, the `DomainModelState` property is updated to match the manager's most recently copy of the domain event model. The `Apply` method should alter the data according to the domain event and the various properties it contains.

The `DomainModelState` is transient and must not be used for any other purpose. After the `Apply` method exits, the manager will reset the property to null.

Domain event handlers must be registered at application startup (see the [Domain Event Handlers Service](services_events.md) topic for more information).

---

[Return to the documentation index](index.md)
