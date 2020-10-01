## Event Stream Collections

The `EventStreamCollection` class is used to create, cache, and access multiple [`EventStreamManager`](ref_eventstreammanager.md) objects. An individual manager instance can be somewhat expensive to initialize and their need for a one-time initialization makes them poorly suited for dependency injection, so the collection class is the recommended way to interact with domain model event streams in general (even if you're not using dependency injection).

For dependency injection scenarios, register the concrete instance of a collection with singleton scope.

### Declaration and Constructors

Collections take a generic parameter representing the domain model root:

`EventStreamCollection<TDomainModelRoot>`

There are two constructors. One is intended for dependency injection usage, which declares dependencies on the three [library services](services.md). The other constructor is designed for non-DI scenarios with one argument, an instance of the [`DirectDependencyServiceHost`](services_direct.md) helper-object used to initialize the library services when DI is not used.

Unlike individual manager objects, the collection does not require a separate initialization call after it is created.

---

### Manager Pass-Through Methods

The collection class has three methods matching those exposed by the `EventStreamManager` class, except that the collection methods have a mandatory `string id` argument to identify which manager should process the request:

* `GetCopyOfState`
* `PostDomainEvent`
* `PostDomainEvents`

Refer to the [`EventStreamManager`](ref_eventstreammanager.md) documentation for details. The other methods in the collection class are related to the underlying collection itself.

---

#### `QueueSize`

This integer property controls the maximum number of managers the collection will store. If a new manager is requested (by specifiying an ID not currently in the collection) and the collection count exceeds this number, the oldest manager in the collection will be removed.

Note that if your application holds a reference to the removed manager, that reference will remain valid. Changing the `QueueSize` property will immediately re-evaluate the collection count.

Set the `QueueSize` to zero to disable the collection count limitation. This value can be set through [policy configuration](configuration_policies.md) at startup.

---

#### `GetEventStreamManager`

This returns a reference to an `EventStreamManager` for the requested unique ID. If the collection does not already contain the requested manager, it will be created, initialized, and added to the collection.

---

#### `ContainsEventStreamManager`

Returns true if the collection contains an `EventStreamManager` for the requested unique ID.

---

#### `GetEventStreamIds`

Returns a `List<string>` representing the unique IDs of the `EventStreamManager` objects currently in the collection.

---

#### `ReleaseEventStreamManager`

If the collection contains a reference to an `EventStreamManager` with the requested unique ID, it will be removed from the collection. An invalid ID will not cause an exception.

---

[Return to the documentation index](index.md)
