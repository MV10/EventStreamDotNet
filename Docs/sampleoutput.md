## Sample Demo Output

This is what the demo project outputs to the console window.

```
EventStreamDotNet demo

Database: Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Database=EventStreamDotNet
Log Table: EventStreamDeltaLog
Snapshot Table: DomainModelSnapshot

Delete all records from the demo database (Y/N)? NO

[11:27:50 DBG] EventStreamConfigService is starting
[11:27:50 DBG] DomainEventHandlerService is starting
[11:27:50 DBG] ProjectionHandlerService is starting
[11:27:50 DBG] Registering configuration for domain model Customer
[11:27:50 DBG] Registering domain event handler CustomerEventHandler for domain model Customer
[11:27:50 DBG]   Caching Apply method for domain event StreamInitialized
[11:27:50 DBG]   Caching Apply method for domain event AccountAdded
[11:27:50 DBG]   Caching Apply method for domain event AccountRemoved
[11:27:50 DBG]   Caching Apply method for domain event CustomerCreated
[11:27:50 DBG]   Caching Apply method for domain event MailingAddressChanged
[11:27:50 DBG]   Caching Apply method for domain event ResidencePrimaryChanged
[11:27:50 DBG]   Caching Apply method for domain event ResidenceSpouseChanged
[11:27:50 DBG]   Caching Apply method for domain event SpouseChanged
[11:27:50 DBG]   Caching Apply method for domain event SpouseRemoved
[11:27:50 DBG]   Caching Apply method for domain event TransactionPosted
[11:27:50 DBG] Registering projection handler CustomerProjectionHandler for domain model Customer
[11:27:50 DBG]   Caching projection method ProjectCustomerResidency for snapshot updates
[11:27:50 DBG]   Caching projection method ProjectCustomerMaritalStatus for domain event SpouseChanged
[11:27:50 DBG]   Caching projection method ProjectCustomerMaritalStatus for domain event SpouseRemoved
[11:27:50 DBG] Created EventStreamCollection for domain model root Customer
Customer id 12345678 exists? True
Retrieving customer.
[11:27:52 DBG] GetEventStreamManager(12345678)
[11:27:52 DBG] Created EventStreamProcessor for domain model root Customer
[11:27:52 DBG] Created EventStreamManager for domain model root Customer
[11:27:52 DBG] Initialize ID 12345678
[11:27:52 DBG] ReadAllEvents ID 12345678
[11:27:52 DBG] ReadSnapshot ID 12345678 loaded ETag 11
[11:27:52 DBG] ApplyNewerEvents ID 12345678
[11:27:52 DBG] EventStreamCollection AddManager for ID 12345678
[11:27:52 DBG] TrimQueue
[11:27:52 DBG] GetCopyOfState(forceRefresh: False)
[11:27:52 DBG] CopyState ID 12345678
Adding spouse (yay).
[11:27:52 DBG] GetEventStreamManager(12345678)
[11:27:52 DBG] PostDomainEvents(deltas: 1, onlyWhenCurrent: False, doNotCopyState: False)
[11:27:52 DBG]   Posting domain event: SpouseChanged
[11:27:52 DBG] WriteEvents ID 12345678 for 1 deltas
[11:27:52 DBG] ReadNewestETag ID 12345678 returning ETag 11
[11:27:52 DBG] WriteAndApplyEvent ID 12345678 for domain event SpouseChanged with ETag 12
[11:27:52 DBG] Domain event handler CustomerEventHandler applied event SpouseChanged to Customer model state
[11:27:52 DBG] Applied event SpouseChanged to promote model ID 12345678 to ETag 12
[11:27:52 DBG] WriteSnapshot ID 12345678 ETag 12
Projecting customer ID 12345678 marital status as MARRIED
Projecting customer ID 12345678 state of residence as TX
[11:27:52 DBG] CopyState ID 12345678
```

---

[Return to the documentation index](index.md)
