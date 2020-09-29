## Configuring Projections

Projections must be configured through code.

Projection is an Event Streaming concept whereby some process periodically extracts data from a domain data model or a snapshot to log or store into some external table or system. These extracts usually provide a source of reporting or query data when it isn't desirable to have the reporting or query system interact directly with the domain model objects or the event stream management library.

This library supports projections triggered by domain events or by snapshot updates. Domain event projections are more specific and therefore usually more efficient because specific events happen less frequently than snapshots in most systems. Consider that a snapshot may be created which has nothing to do with the data that the projection is interested in.

Domain event projection handlers are called in the order in which the events occur, and any snapshot handlers are called afterwards. A handler is only called once for any given batch of domain events.

In the demo project, refer to the `CustomerProjections` class for examples of projection handlers.

A projection method is nothing more than a void-returning (probably `async void`, see below) which takes one argument matching the domain model root object, which represents the current state of the domain model object. The projection is free to use that data however it likes.

### Exception Handling is Critical

Since projections typically write data, they are probably async methods. However, they work like an event handler, which does not return a .NET `Task`. This is the `async void` pattern and event handlers are the only place they are considered appropriate. However, it is _critical_ that the method catches _all_ exceptions. If an exception is unhandled, the .NET runtime will immediately halt the process.

---

#### `AddSnapshotHandler`

This accepts a single argument representing the method to invoke when the snapshot is updated. You can see an example of this call in `Program.Main` in the demo project.

---

#### `RemoveSnapshotHandler`

This accepts a single argument representing the snapshot projection method to remove from the collection.

---

#### `AddDomainEventHandler`

This accepts a type parameter defining the domain event which triggers the handler, and an argument representing the method to invoke when the domain event has been stored and applied. You can see an example of this call in `Program.Main` in the demo project.

---

#### `RemoveDomainEventHandler`

This accepts a single argument representing the domain event projection method to remove from the collection.

---

[Return to the Configuration topic](configuration.md)
