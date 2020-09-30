## Quick Start

This covers the basics that you need to know to begin using EventStreamDotNet.

### Database Preparation

The repository includes `tables.sql` which defines the two tables required by the library. If you only have a single domain data model to represent, you can use this as-is. Review the comments in the file, you can alter the size of the `Id` fields, and you can alter the table names. If your system will represent multiple domain data models, you will need to add additional copies of the two tables to host the other models (with different table names, of course).

### Library Configuration

The library defines a set of configuration classes designed to be populated by the _`Microsoft.Extensions.Configuration.*`_ packages. The root of this hierarchy is `EventStreamDotNetConfig` and the only required group of values are in `DatabaseConfig`, namely, the database connection string, the event table name, and the snapshot table name. Refer to the demo project's `appsettings.json` and `AppConfig` class for a look at how to quickly define and load your desired configuration. Later we'll discuss how to register the configuration with the library (more advanced scenarios can load multiple configurations for working with multiple domain models simultaneously).

### Create a Domain Data Model

If you're unfamiliar with the concept of domain data models, you should review the [Architectural Patterns](patterns.md) page first. It is critical that you get the model right before you begin using it. The model classes must be POCOs with default constructors (that is, a constructor without arguments, which allows the objects to be serialized and deserialized). The root class of your domain model hierarchy must implement the library's `IDomainModelRoot` interface, which requires the class to have a `string Id` property. The demo project's `DomainData\DataModel` folder contains an example of a complete domain data model. The `Customer` class is the domain model root.

### Create Domain Events

You must define a list of domain events as simple classes. Like the domain data model, if this concept is unfamiliar to you, it's important that you review the [Architectural Patterns](patterns.md) page first. Each domain event class must drive from the library's `DomainEventBase` abstract class. This will add several properties the library requires, but does not require you to implement anything. The demo project's `DomainData\DomainEvents` folder contains examples of domain events.

### Create a Domain Event Handler

Finally, you must write a class to handle domain events. This is invoked by the library to apply domain events to a copy of the domain model's state. Keep in mind that conceptually these events are assumed to have already happened when the library invokes this class. That means higher-order business rules have already been considered and "approved" before the command was issued that led to this event. The sole responsibility of this class is to apply the change.

The class must implement the library's `IDomainModelEventHandler<TDomainModelRoot>` interface, which requires a public property of the domain model root class type called `DomainModelState`, and a specific `Apply` method for the library's special `StreamInitialized` property. The class must also provide `Apply` methods for every possible domain event (each method accepts one of the domain events as the method argument). The library discovers and invokes these through reflection. The demo project has an example of this in the `CustomerEventHandler` class.

Note that the reference stored in the `DomainModelState` property is temporary. The library will set the reference before calling an `Apply` method, then it will clear the reference (setting it to `null`) immediately after the call returns. This ensures the client application can't depend on the property for any other purpose.

Next we'll discuss how to register the domain event handler with the library.

### Library Services

Three important library services are provided to manage domain-model-specific library configurations, domain event handlers, and projection handlers: `EventStreamConfigService`, `DomainEventHandlerService`, and `ProjectionHandlerService`. Even if your application only uses a single domain model, you must register your classes with these services. Projections are optional (although the service must still exist), but use of the config and event handler services is always required. The config service must be configured first because the other two services depend upon it.

How you access the services, and how the library accesses them depends on whether you're using dependency injection (DI). The use of DI is completely optional. If you are using DI, create and configure each individual service object, register your classes as described below, then register them with the service container with a singleton scope. (Register the concrete instances, there are no separate interfaces.) If you are not using DI, create the `DirectDependencyServiceHost` object, then access the services through the three properties exposed by the host.

To register your classes with the services, first, call the `AddConfiguration` method on the `EventStreamConfigService` object. It requires you to identify the domain model root class and the populated library configuration data in an `EventStreamDotNetConfig` object. Next, call the `RegisterDomainEventHandler` method on the `DomainEventHandlerService` object. It requires you to identify the domain model root class and the domain event handler class.

Refer to the repository's demo project for an example of both DI and non-DI approaches.

### Decision: One or Many Domain Objects?

The library offers two approaches for working with the domain data model. The `EventStreamManager` is tied to a single specific domain object (that is, an _instance_ of the domain data model), and therefore a single specific ID value. This is equivalent to working with a single record in a traditional database model -- one customer, in the context of the demo project. Alternately, you can use `EventStreamCollection` which manages multiple copies of `EventStreamManager`. They have the same general interface, but the collection version accepts an ID argument to control which specific domain data object you're working with. The collection approach is more appropriate for scenarios like batch processing.

For DI scenarios, you will probably want to register one or more `EventStreamCollection` objects as singletons, and let the collection objects deal with the individual manager objects. You probably don't want to register `EventStreamManager` objects for injection since each object represents a single specific instance of the domain data. (Rare exceptions may apply, such as data about the specific machine where the application is running.)

For non-DI scenarios, both classes have a constructor that accepts the `DirectDependencyServiceHost` object described in the previous section. This allows the objects to obtain references to the three required library services.

**IMPORTANT:** If you're using the single-object `EventStreamManager` class, you must also await the `Initialize` method before using the manager. This establishes the unique ID and performs certain asynchronous database operations that are not appropriate in a constructor. Calling other manager methods without calling `Initialize` will throw exceptions. It is not necessary to initialize the collection class.

### The "Always Exists" Approach

Each instance of your domain data model has a unique ID, and the library is designed to operate as if the ID "always exists". That means when you create a new `EventStreamManager` for a given ID, if that ID doesn't already exist in the database, the manager will write the special `Stream Initialized` domain event and create a new snapshot of the object.

If the ID does already exist in the database, during initialization the manager will read the snapshot, then apply any newer events since the snapshot was created, then produce a new snapshot.

If you need to check for an ID in advance, a simple query against the event stream table will accomplish this. The library doesn't currently have anything to do this for you. Similarly, in keeping with traditional interpretations of Event Streams, the library doesn't have a way to delete a given ID (e.g. removing the domain data object itself). If you need that capability, it's simple enough to write a query to do this -- but you must remove _all_ records for the ID. Never delete an individual domain event record.

### Common Functionality

Both the `EventStreamManager` and `EventStreamCollection` classes primary functionality is the same, except that the collection methods also accept an ID argument. If the requested ID hasn't been seen before, the collection object will automatically create and initialize an `EventStreamManager` for that ID, but in general the collection object just passes these common method calls through to the underlying manager object.

Refer to the API pages for details, but a brief description of the common methods follows:

* `GetCopyOfState` - returns a copy of the manager's domain data object
* `PostDomainEvent` - logs a single new domain event to the stream, returns an updated domain data model
* `PostDomainEvents` - logs multiple new domain events to the stream, returns an updated domain data model

Refer to the CQRS classes in the demo project for examples of using these.

### EventStreamManager Functionality

The class members specific to `EventStreamManager` have already been discussed:

* `Id` - a string property representing the unique ID for this domain model instance
* `Initialize` - the first method you must invoke before  using the manager object

### EventStreamCollection Functionality

The collection object adds features for working with the underlying collection:

* `QueueSize` - an integer property that controls how many managers the collection will hold; set to zero for unlimited
* `GetEventStreamManager` - returns a manager for the given ID (either from the collection or newly initialized)
* `ContainsEventStreamManager` - returns true if the collection already holds a manager for the given ID
* `ReleaseEventStreamManager` - if the collection has a reference to the given ID, that reference is removed
* `GetEventStreamIds` - returns a list of the Event Stream IDs currently held in the collection

---

[Return to the documentation index](index.md)
