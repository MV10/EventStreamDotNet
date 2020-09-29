## Database Configuration

The library's database configuration object is pretty simple. It is the only configuration element that is absolutely required. The `EventStreamManager` and `EventStreamCollection` classes will throw an exception if the database configuration object is not populated.

Refer to the comments in the `tables.sql` file in the repository for details about what you can customize.

---

#### `ConnectionString`

Not surprisingly, this property should contain a standard SQL Server connection string. The library doesn't require a stand-alone database, although certain high-volume scenarios may benefit from isolation.

---

#### `EventTableName`

This must reflect the name of the table where domain events are stored. Client applications can read from this table but should never write to it. Reading from the table should always be ordered by the `ETag` column.

---

#### `SnapshotTableName`

This must reflect the name of the table where domain model snapshots are stored. Client applications can read from this table but should never write to it.

---

[Return to the Configuration topic](configuration.md)
