## Welcome to EventStreamDotNet

EventStreamDotNet is a simple, easy-to-use library that supports the Event Stream pattern for storing business data (also commonly known as Event Sourcing). It is based upon .NET Core 3.1 (soon to be .NET 5) and SQL Server. The README covers all the basic points about what it does and what the client applications do. The documentation (linked below) provides a brief overview of the Event Stream pattern and the often-related Domain Driven Design (DDD) and Command Query Responsibility Separation (CQRS) patterns -- which both naturally lead to Event Stream usage.

This is a completely separate stand-alone library, although you may be interested in my article about implementing a similar system using the Microsoft Orleans virtual actor model framework: [Event Sourcing with Orleans Journaled Grains](https://mcguirev10.com/2019/12/05/event-sourcing-with-orleans-journaled-grains.html).

#### General Information

* [Quick Start](quickstart.md)
* [Configuration](configuration.md)
  * [The Basics](configuration_basics.md)
  * [Database](configuration_db.md)
  * [Policies](configuration_policies.md)
  * [Projection](configuration_projections.md)
  * [Debug Logging](configuration_logging.md)
* [Architectural Patterns](patterns.md)
* Dependency Injection
  * Using With Dependency Injection
  * Use Without Dependency Injection
  * ASP.NET DI Service Support
* [Sample Demo Output](sampleoutput.md)

#### Reference Material

* Domain Data Model Support
* Domain Event Support
* Projection Support
* Event Stream Manager
* Event Stream Collection

---

[Return to the repository](https://github.com/MV10/EventStreamDotNet)
