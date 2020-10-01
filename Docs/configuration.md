## Configuration

The subject of "configuration" has two aspects -- the various settings which are most commonly represented in a configuration file (most likely `appsettings.json`), and the setup of several library services which must be configured in code. This section is about the former. For the latter, please refer to the [Library Services](services.md) documentation.

The library defines a set of configuration classes designed to be populated by the _`Microsoft.Extensions.Configuration.*`_ packages (aka _MEC_). The root of this hierarchy is `EventStreamDotNetConfig` and the only required group of values are in `DatabaseConfig`, namely, the database connection string, the event table name, and the snapshot table name. However, there are several other settings you can control.

### Topics

* [The Basics](configuration_basics.md)
* [Database](configuration_db.md)
* [Policies](configuration_policies.md)
* [Projection Handlers](configuration_projections.md)
* [Debug Logging](configuration_logging.md)

---

[Return to the documentation index](index.md)
