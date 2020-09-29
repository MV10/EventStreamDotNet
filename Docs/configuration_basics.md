## Configuration

The library defines a set of configuration classes designed to be populated by the _`Microsoft.Extensions.Configuration.*`_ packages (aka _MEC_). The root of this hierarchy is `EventStreamDotNetConfig` and the only required group of values are in `DatabaseConfig`, namely, the database connection string, the event table name, and the snapshot table name. However, there are several other settings you can control, and there are some configuration points which must be handled during startup in code.

The hierarchy of the configuration elements is shown below:

```
EventStreamDotNet
   |-- Database
   |      |-- ConnectionString
   |      |-- EventTableName
   |      \-- SnapshotTableName
   |
   |-- Policies
   |      |-- SnapshotFrequency
   |      |-- SnapshotInterval
   |      \-- DefaultCollectionQueueSize
   |
   |-- ProjectionHandlers
   |
   \-- LoggerFactory
```

A typical `appsettings.json` configuration (taken from the demo project) looks like this:

```json
{
  "EventStreamDotNet": {
    "Database": {
      "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=EventStreamDotNet",
      "EventTableName": "EventStreamDeltaLog",
      "SnapshotTableName": "DomainModelSnapshot"
    },
    "Policies": {
      "SnapshotFrequency": "AfterAllEvents",
      "DefaultCollectionQueueSize":  10
    }
  }
}
```

The demo also shows how easy it is to automatically read that configuration:

```csharp
public class AppConfig
{
    public static AppConfig Get { get; private set; }

    public static void LoadConfiguration()
    {
        Get = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .Get<AppConfig>(); 
    }

    public EventStreamDotNetConfig EventStreamDotNet { get; set; }
}
```

THe _MEC_ package maps config sections to properties by name, so "EventStreamDotNet" in the JSON is matched up with the `AppConfig.EventStreamDotNet` property by the same name. This means you can reference multiple configuration blocks by customizing the top-level name and the properties in your config class:

```json
{
  "CustomerEventStream": {
    "Database": {
      "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=EventStreamDotNet",
      "EventTableName": "CustomerDeltaLog",
      "SnapshotTableName": "CustomerSnapshot"
    },
    "Policies": {
      "SnapshotFrequency": "AfterAllEvents",
      "DefaultCollectionQueueSize":  10
    }
  },
  "HumanResourcesEventStream": {
    "Database": {
      "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=EventStreamDotNet",
      "EventTableName": "HumanResourcesDeltaLog",
      "SnapshotTableName": "HumanResourcesSnapshot"
    },
    "Policies": {
      "SnapshotFrequency": "AfterIntervalSeconds",
      "DefaultCollectionQueueSize":  30
    }
  }
}
```

Reading both is as simple as updating the properties to match:

```csharp
public class AppConfig
{
    public static AppConfig Get { get; private set; }

    public static void LoadConfiguration()
    { ...omitted... }

    public EventStreamDotNetConfig CustomerEventStream { get; set; }

    public EventStreamDotNetConfig HumanResourcesEventStream { get; set; }
}
```

---

[Return to the Configuration topic](configuration.md)
