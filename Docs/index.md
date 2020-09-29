## Welcome to EventStreamDotNet

EventStreamDotNet is a simple, easy-to-use library that supports the Event Stream pattern for storing business data (also commonly known as Event Sourcing). It is based upon .NET Core 3.1 (soon to be .NET 5) and SQL Server. The README covers all the basic points about what it does and what the client applications do. The documentation (linked below) provides a brief overview of the Event Stream pattern and the often-related Domain Driven Design (DDD) and Command Query Responsibility Separation (CQRS) patterns -- which both naturally lead to Event Stream usage.

This is a completely separate stand-alone library, although you may be interested in my article about implementing a similar system using the Microsoft Orleans virtual actor model framework: [Event Sourcing with Orleans Journaled Grains](https://mcguirev10.com/2019/12/05/event-sourcing-with-orleans-journaled-grains.html).

> **NOTE** The library was recently refactored to be dependency-injection friendly. The documentation has not been updated to reflect those changes yet. When in doubt, refer to the demo project until this notice is removed.

#### General Information

* [Architectural Patterns](patterns.md)
* [Quick Start](quickstart.md)
* [Configuration](configuration.md)
  * [The Basics](configuration_basics.md)
  * [Database](configuration_db.md)
  * [Policies](configuration_policies.md)
  * [Projection Handlers](configuration_projections.md)
  * [Debug Logging](configuration_logging.md)
* Dependency Injection
  * Using Dependency Injection
  * Non-DI Client Applications
  * ASP.NET DI Service Support

#### Reference Material

* Domain Data Model Support
* Domain Event Support
* Event Stream Manager
* Event Stream Collection

---

[Return to the repository](https://github.com/MV10/EventStreamDotNet)
