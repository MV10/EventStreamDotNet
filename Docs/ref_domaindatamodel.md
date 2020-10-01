## Domain Data Model Support

Domain data models are central to event stream systems (if you are not familiar with these concepts, please review the [Architectural Patterns](patterns.md) page first). In a nutshell, the domain data model defines the entire set of objects and relationships needed to operate a particular area of the business.

All the values and objects in your domain model must be public properties with both `get` and `set` publicly available. All classes must have default constructors (that is, constructors without arguments). These conditions allow the library to serialize and deserialize the objects in your model.

The EventStreamDotNet library provides the `IDomainModelRoot` interface. The root class of your domain data model must inherit from this interface. It requires that your root class provides a `string Id` property which represents the unique ID associated with the instance of the domain data model.

Many of the classes and methods in the library take a `TDomainModelRoot` generic parameter. You should provide the name of your domain model root class in these cases. For example:

```csharp
public class Customer : IDomainModelRoot
{
    public string Id { get; set; } // required by the interface
    public string Name { get; set; }
    // etc.
}

domainEventHandlers.RegisterDomainEventHandler<Customer, CustomerEventHandler>();
// Customer is the <TDomainModelRoot> parameter ^above
```

You must never directly modify the data in a copy of the domain model data you retrieve from the library. Instead you should issue domain events to describe desired changes to the data model. These are issued using the `PostDomainEvent` methods on the [`EventStreamManager`](ref_eventstreammanager.md) and [`EventStreamCollection`](ref_eventstreamcollection.md) classes.

---

[Return to the documentation index](index.md)
