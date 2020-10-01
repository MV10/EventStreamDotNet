## Event Streams and Dependency Injection

For applications which use dependency injection, the recommended approach is to register an `EventStreamCollection` for injection with a singleton scope. It is not necessary to register a concrete instance, the collection object has a DI-friendly constructor which will receive the library services from the DI container.

Although it is possible to register individual `EventStreamManager` objects for DI, this is rarely a useful pattern. You must only register concrete instances, as DI instantiation is not ideal given the need to call `Initialize` on new manager objects (but you can do this in a constructor, it is safe to call multiple times). An example of a case where injection of a single manager might make sense is a domain model that represents the specific server on which the application is running.

There is no downside to accessing managers through a collection. The overhead of the collection layer is almost immeasurably small.

---

[Return to the documentation index](index.md)
