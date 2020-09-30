# EventStreamDotNet

A simple delta-logging Event Stream library for .NET and SQL Server. This is a work-in-progress, although it is largely complete. I will publish a NuGet package when I'm satisfied with the library behavior. The demo project is fairly extensive and shows how to use all major features.

Refer to the documentation [index](Docs/index.md) for more information (docs are also a work-in-progress).

The general plan is typical of Event Stream systems:

* the domain data model has a single POCO as the root object
* a POCO object graph represents the complete domain model
* the root object can be represented by a unique string ID
* changes to the domain model are represented by domain event POCOs
* domain events are past-tense in the sense of applied CQRS commands
* snapshotting is handled internally by the event stream manager
* projections can be driven off applied events or snapshot updates

The client application is responsible for providing:

* the domain data model object graph
* the domain event classes
* the domain event handler (applies event changes to the state)
* optional projection handlers
* services (likely CQRS) for interacting with the event stream manager

Package dependencies:

* JSON.Net
* Microsoft.Extensions.DependencyInjection
* Microsoft.Extensions.Logging
* System.Data.SqlClient
