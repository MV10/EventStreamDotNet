# EventStreamDotNet

A free, easy-to-use Event Stream library for .NET and SQL Server. 

* [Documentation](Docs/index.md)
* [NuGet package version 1.0.0](https://www.nuget.org/packages/EventStreamDotNet/1.0.0)

Features:

* delta logging (domain events represent data model revisions)
* version (ETag) validation
* multiple domain data models can be managed within one application
* configurable snapshot policies
* snapshotting is handled internally by the event stream manager
* projections can be driven off applied events or snapshot updates

The client application is responsible for providing:

* the domain data model object graph
* the domain event classes (describes changes to the data model)
* the domain event handler (applies event changes to the state)
* optional projection handlers (extracts data after changes applied)
* services (likely CQRS) for interacting with the event stream manager

Requirements:

* the domain data model has a single POCO as the root object
* a POCO object graph represents the complete domain model
* the root object can be represented by a unique string ID
* changes to the domain model are represented by domain event POCOs
* domain events are past-tense (in the sense of applied CQRS commands)

Package dependencies:

* JSON.Net
* Microsoft.Extensions.DependencyInjection
* Microsoft.Extensions.Logging
* System.Data.SqlClient
