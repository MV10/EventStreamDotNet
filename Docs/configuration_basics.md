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
   \-- Projection
          \-- ConnectionString
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
    },
    "Projection": {
      "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=EventStreamDotNet"
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

THe _MEC_ package maps config sections to properties by name, so "EventStreamDotNet" in the JSON is matched up with the `AppConfig.EventStreamDotNet` property by the same name. This means you can reference multiple configuration blocks by customizing the top-level name and the properties in your config class, and each configuration block can be named anything that makes sense in your system:

```json
{
  "CustomerModelSettings": {
    "Database": { ... },
    "Policies": { ... },
    "Projection": { ... }
  },
  "HumanResourcesModelSettings": {
    "Database": { ... },
    "Policies": { ... },
    "Projection": { ... }
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

    public EventStreamDotNetConfig CustomerModelSettings { get; set; }

    public EventStreamDotNetConfig HumanResourcesModelSettings { get; set; }
}
```

The library will only care about the contents of those configuration objects, not what you named it or how it was populated.

---

[Return to the Configuration topic](configuration.md)
