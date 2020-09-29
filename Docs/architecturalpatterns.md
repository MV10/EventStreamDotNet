## Architectural Patterns

Event Streaming (ES), also sometimes called Event Sourcing, is an architectural pattern for storing business data. There are different interpretations of the concept and different ways to implement it. This library takes an approach known as delta logging. It is closely related to another set of architectural patterns grouped under the heading Domain Driven Design (DDD), and both ES and DDD are commonly related to a third architectural pattern, Command Query Responsibility Separation, or CQRS, although this last is not strictly necessary to use the library. (The Internet is full of arguments about correct interpretations of these patterns, so I will simply describe what the patterns mean in the context of the EventSourceDotNet library.)

### Domain Driven Design (DDD)

Before you ever use ES, you need a solid domain data model. This is just a group of related C# data classes (meaning they only expose properties, no methods, no constructors, sometimes called POCOs) that represent the business data you're working with and the relationship between those business objects. That model and the connections between them is collectively known as the _object graph_ and you must identify a single class as the _domain model root_.

The demo program in the repository models a simple banking application. The complete domain data model is shown below. Notice the model is entirely business-oriented terminology. A common mistake is to "pollute" your domain model with technology concerns (for example, temporarily storing UI data paging keys). Nothing about these patterns prohibits the use of other data sources -- if the data isn't crucial to the operation of the business, it doesn't belong in your domain data model. It's easy to add things to the model, but it's difficult and often impossible to ever get rid of them again.

The domain model root _must_ have a unique identifier. In this library, that ID must be a string value. The demo model identifies the `Customer` as the domain model root. In that system, a single customer may hold multiple accounts. Other systems commonly work the other way around, starting from an account which relate to one (or perhaps multiple) people. A utilities billing system may be rooted on an address. Any other model is possible, as long as it accurately reflects the business needs.

Finally, more complex systems may have multiple domain data models. Generally you want to strive to avoid overlaps between these models. (In Astronaut Architect terminology, a group of related domains and how they relate is known as a Bounded Context, but that is beyond the scope of this documentation.) The library doesn't help you with model-to-model relationships, but it can be used in a single application to manage more than one domain model at the same time.

```
Customer (class, root)
  |--Id
  |--PrimaryAccountHolder (Person class)
  |    |--FullName
  |    |--FirstName
  |    |--LastName
  |    |--Residence (Address class)
  |    |    |--Street
  |    |    |--Street2
  |    |    |--City
  |    |    |--StateOrProvince
  |    |    |--Country
  |    |    \--PostalCode
  |    |
  |    |--TaxId
  |    \--DateOfBirth
  |
  |--Spouse (Person class)
  |    |--FullName
  |    |--FirstName
  |    |--LastName
  |    |--Residence (Address class)
  |    |    |--Street
  |    |    |--Street2
  |    |    |--City
  |    |    |--StateOrProvince
  |    |    |--Country
  |    |    \--PostalCode
  |    |
  |    |--TaxId
  |    \--DateOfBirth
  |
  |--MailingAddress (Address class)
  |    |--Street
  |    |--Street2
  |    |--City
  |    |--StateOrProvince
  |    |--Country
  |    \--PostalCode
  |
  \--Accounts (Account class list)
       |--IsPrimaryAccount
       |--AccountType
       |--AccountNumber
       \--AccountBalance
```

### Command Query Responsibility Separation

CQRS is just a design pattern for a service layer. One architectural approach is to publicly expose a higher-level business-function-focused API (called the Aggregation Pattern and/or Gateway Pattern). Those hide the CQRS layer, which is generally a pair of services, one focused on _commands_ (service calls that do something to the data model), and the other focused on _queries_ (services calls that read data). These can point to different data sources. Often in high-transaction-volume systems, command services will use a write-optimized data store, while query services use a read-optimized store (see the _Projection_ topic for more details about how those data stores would be related). 

As mentioned earlier, CQRS isn't strictly necessary to use this library. However, many developers discover that the pattern follows very naturally from the use of DDD and ES, and it also happens to be a convenient organizational approach to your projects based on the way a CQRS service layer interacts with ES.

### Event Streams

Architects talk about the ES pattern with phrases like "source of truth", but the fact is that an ES is just an append-only list of all the changes that have ever been made to the data model. Full stop. That's all it is. In fact, you've probably used ES many times without knowing it -- many database systems like SQL Server are built on top of an event stream called the transaction log. The "append-only" factor is critically important. In the ES world, you never delete data, you only add more changes, some of which may document the undoing of some previous change. The UI presentation will be deletion, but the original data and the change to remove it will both be in the ES transaction log forever. This is why we have said the library is a _delta logging_ system -- it records changes.

The contents of the event stream data table are called _domain events_, which are always past-tense descriptions of a change that has already been applied to the domain data model. The trick to designing ES effectively is to find the right "resolution" for these changes. The demo has a somewhat complicated domain data model so that this principle can be shown more effectively. The data classes are relatively compact and simple, and changes at the level of those classes are how most of the domain events are defined. So if the customer moves, that might produce a `MailingAddressChanged` event _and_ a `ResidencePrimaryChanged` event. The events are also classes which contain the details of the change, so each of those domain events carry an `Address` object reflecting the new address data for the change.

Like the domain data model itself, domain events must be expressed in business terms and should never represent non-business technology concerns -- use traditional database tables and techniques outside the DDD / ES system for that type of thing. The ES library uses a special-case domain event called `Stream Initialized` which is always the first event written for a given ID. This effectively represents the creation of the domain data model's root object. The demo defines the following domain events, and as you can see, their purpose is clear and easy to understand.

| Domain Event              | Payload Describing the Change |
| ------------------------- | ----------- |
| Customer Created          | `Person` PrimaryAccountHolder, `Address` MailingAddress |
| Account Added             | `Account` |
| Account Removed           | `string` AccountNumber |
| Mailing Address Changed   | `Address` MailingAddress |
| Residence Primary Changed | `Address` |
| Residence Spouse Changed  | `Address` |
| Spouse Changed            | `Person` Spouse |
| Spouse Removed            | (nothing) |
| Transaction Posted        | `string` AccountNumber, `double` Amount, `double` OldBalance, `double` NewBalance |

The library will store these events in the database, which represents how the data model changes over time. In an ES system, it is possible to "re-play" the sequence of events to arrive at the current state of the data model. One such sequence might look like this:

```
0 - Stream Initialized
1 - Customer Created
2 - Spouse Changed
3 - Account Added
4 - Transaction Posted
5 - Transaction Posted
6 - Transaction Posted
7 - Mailing Address Changed
8 - Residence Primary Changed
9 - Residence Spouse Changed
```

Events can be written individually or in groups. For example, if the primary account holder and their spouse live at the same address, when they move, a batch of three domain events may be needed (7, 8, 9). These are still always recorded as a sequence. Should circumstances require removing the spouse from the system, a new `Spouse Removed` event #10 will be added to the end of the stream. Event #2, `Spouse Changed` will still always exist as part of the history of this instance of the domain data model.

### Snapshots

Most ES systems support the concept of "snapshots" -- a point-in-time copy of the domain model state. In practice, you don't want to load and replay every event in the history of a given domain object, so ES systems have various ways to update a stored snapshot as new events are posted to the event log. It is important that the client application is written with the understanding that the snapshot is only a copy and may not reflect the latest events in the log. (This is what is meant by the event stream itself being "the source of truth" -- the snapshot is _not_ the "real" data, only the event stream is.)

It is also important that the client application does not modify any copy of the domain data it receives. Instead, it must issue commands which produce some sort of domain event, describing the changes that resulted from the command. Typically issuing a command also returns an updated copy of the domain model -- again, just a copy, which may or may not also be stored into the database as a snapshot.

The technical term for this architectural pattern is Eventual Consistency. This is the idea that not all systems are always in perfect synchronization. Programmers don't like this, it seems messy, but interestingly, business users are normally comfortable with it -- this is how the real world works. An account balance won't immediately reflect a deposit. Concepts like accounts receivable are directly related. In the real world, it is often desirable to keep that snapshot as up-to-date as possible, and the library does support snapshot updates after every event or every batch of events. With modern database technology and server performance, the overhead is rarely significant.