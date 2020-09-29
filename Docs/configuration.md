## Configuration

The library defines a set of configuration classes designed to be populated by the _`Microsoft.Extensions.Configuration.*`_ packages (aka _MEC_). The root of this hierarchy is `EventStreamDotNetConfig` and the only required group of values are in `DatabaseConfig`, namely, the database connection string, the event table name, and the snapshot table name. However, there are several other settings you can control, and there are some configuration points which must be handled during startup in code.

[Return to the Index](index.md)

### Topics

* [The Basics](configuration_basics.md)
* [Database](configuration_db.md)
* [Policies](configuration_policies.md)
* [Projection Handlers](configuration_projections.md)
* [Debug Logging](configuration_logging.md)

