## Event Stream Managers

The purpose of an Event Stream architecture is to represent the sequence of domain events that modify the state of the domain model object. The library's `EventStreamManager` is the class which manages the events and current state of a single specific instance of the domain data model. Although you can directly create individual manager objects, the recommended usage pattern is to obtain references to them from an [`EventStreamCollection`](ref_eventstreamcollection.md).

Because each manager represents just one domain object (and the associated domain event stream and snapshot), the manager object has a `string Id` property that represents the unique ID for the domain model root object.

Generally you should not register individual `EventStreamManager` instances for dependency injection. Their required initialization pattern is not a good fit for storage or creation by DI containers. Instead, register a singleton collection for DI and obtain your manager objects from the collection. (If you prefer, you can even configure the collection to hold just one manager, the overhead of the collection layer is nearly non-existent.)

### Declaration and Initialization

Managers take a generic parameter representing the domain model root:

`EventStreamManager<TDomainModelRoot>`

There are two constructors. One is DI-friendly (but not recommended, as described above) which declares dependencies on the three [library services](services.md). The other constructor is designed for non-DI scenarios with one argument, an instance of the [`DirectDependencyServiceHost`](services_direct.md) helper-object used to initialize the library services when DI is not used.

Immediately after obtaining a reference to a manager object, you should await the `Initialize(id)` method. The manager will read the latest snapshot, then apply any newer domain events to bring the internal model state up to date. Invoking any other methods before initialization will result in an exception.

---

#### `GetCopyOfState`

This method returns a _copy_ of the current state of the domain model object. Generally this will be up to date except when another process may have written additional domain events under the same ID.

The optional `bool forceRefresh` argument can be set to true to require the manager to check for and apply new events before returning the copy.

As emphasized throughout the documentation, the application shouldn't modify the copy (it won't have any effect on the "real" model state, nor can a modified copy be saved in any library-compatible way), nor should the application hold a reference to the copy long-term. Instead, new copies should be obtained and used locally as needed. Copying is very fast (internally, JSON.NET serializes then deserializes the domain model to produce the stand-alone copy).

This method also exists in the `EventStreamCollection` interface, but with a mandatory `string id` argument.

---

#### `PostDomainEvent` 

A convenience method which forwards a single domain event to `PostDomainEvents` (below).

---

#### `PostDomainEvents`

This is how applications alter the state of a domain object.

These methods are used to post one or more changes to the domain model state in the form of [domain event](ref_domainevents.md) objects. The changes are stored and applied in the sequence provided. The domain model's snapshot policy is applied, and any dependent [projections](ref_projections.md) will be invoked. The domain events are provided in the single `DomainEventBase delta` argument or the `List<DomainEventBase> deltas` argument.

The methods return a `(bool Success, TDomainModelRoot CopyOfCurrentState)` tuple.

An optional `bool onlyWhenCurrent` argument can be set true to require the manager to check for newer events that have been stored in the database (by another process, most likely) which have not been applied to the manager's current model state. If newer events are found, the call immediately exits with a false `Success` return value. You can force the manager to synchronize by calling `GetCopyOfState` with a true `forceRefresh` argument.

The `onlyWhenCurrent` flag is optional because some domain events are not sensitive to current state. For example, a banking application should always be able to store a deposit transaction, whereas a withdrawal transaction would be state-dependent since it should be blocked if the withdrawal would create an overdraft.

An optional `bool doNotCopyState` argument can be set to true to skip setting the `CopyOfCurrentState` return value (it will contain null). This is useful in high-volume batch-style processing and intermediate state changes are not of interest.

This method also exists in the `EventStreamCollection` interface, but with a mandatory `string id` argument.

---

[Return to the documentation index](index.md)
