
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    internal class EventStream<TDomainModelRoot, TDomainEventHandler>
        where TDomainModelRoot : class, IDomainModelRoot, new()
        where TDomainEventHandler : class, IDomainModelEventHandler<TDomainModelRoot>, new()
    {
        /// <summary>
        /// The unique ID assigned to this event stream.
        /// </summary>
        internal readonly string Id;

        /// <summary>
        /// The entity tag (aka version number) for the domain model state stored in this object instance.
        /// </summary>
        internal long ETag = 0;

        /// <summary>
        /// Represents the ETag last read from the snapshot table. Can be used to decide if/when to
        /// output a new snapshot.
        /// </summary>
        internal long LastKnownShapshotETag;

        /// <summary>
        /// Represents the interval since this object instance wrote a new snapshot.
        /// </summary>
        internal Stopwatch LastSnapshotDuration = new Stopwatch();

        /// <summary>
        /// EventStreamDotNet configuration.
        /// </summary>
        internal readonly EventStreamDotNetConfig Config;

        /// <summary>
        /// False until Initialize is called, public/internal methods will throw until then.
        /// </summary>
        internal bool IsInitialized;

        /// <summary>
        /// Domain model state is kept private to ensure only copies can be retrieved (using method
        /// names that will remind the developer of the client app that they only obtained a copy.)
        /// </summary>
        private TDomainModelRoot State;

        // TODO - do we need to keep this around or will ApplyMethods hold the ref alive?
        private readonly IDomainModelEventHandler<TDomainModelRoot> EventHandler;

        /// <summary>
        /// During initializtion all Apply methods in the EventHandler are stored, keyed on the domain event they process.
        /// </summary>
        private Dictionary<Type, MethodInfo> ApplyMethods;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The unique identifier for this domain model object and event stream.</param>
        /// <param name="config">The configuration for this event stream.</param>
        internal EventStream(string id, EventStreamDotNetConfig config)
        {
            Id = id;
            Config = config;
            EventHandler = Activator.CreateInstance<TDomainEventHandler>();
            IsInitialized = false;
        }

        /// <summary>
        /// Reads the snapshot and all newer events. See <see cref="ReadAllEvents"/> for details
        /// of how the snapshot may be updated.
        /// </summary>
        internal async Task Initialize()
        {
            ApplyMethods = new Dictionary<Type, MethodInfo>();
            var methods = EventHandler.GetType().GetMethods();
            foreach(var method in methods)
            {
                if(method.Name == "Apply")
                {
                    var type = method.GetParameters()[0].ParameterType;
                    ApplyMethods.Add(type, method);
                }
            }

            await ReadAllEvents();
            IsInitialized = true;
        }

        /// <summary>
        /// Returns a copy of the currently known state. Does not attempt to synchronize with newer
        /// events that may have been written to this stream by other clients.
        /// </summary>
        internal TDomainModelRoot CopyState()
        {
            if (!IsInitialized) throw new Exception("The EventStream has not been initialized");

            var json = JsonConvert.SerializeObject(State, JsonSettings);
            return JsonConvert.DeserializeObject<TDomainModelRoot>(json, JsonSettings);
        }

        /// <summary>
        /// Reads the snapshot, if any, then applies any newer events to the model. The snapshot is always
        /// updated by this call, if necessary, regardless of the configured snapshot update policy.
        /// The return value indicates whether the snapshot was updated.
        /// </summary>
        internal async Task<bool> ReadAllEvents()
        {
            if (!IsInitialized) throw new Exception("The EventStream has not been initialized");

            using var connection = new SqlConnection(Config.Database.ConnectionString);
            await connection.OpenAsync();

            // get the latest snapshot
            (ETag, State) = await ReadSnapshot(connection);
            LastKnownShapshotETag = ETag;
            LastSnapshotDuration.Restart();

            // apply events newer than this object instance's current state
            var oldETag = ETag;
            await ApplyNewerEvents(connection);

            // if newer events were found, update the snapshot
            bool appliedNewerEvents = (oldETag != ETag);
            if(appliedNewerEvents)
            {
                await WriteSnapshot(connection);
            }

            await connection.CloseAsync();

            // indicate whether a snapshot was updated
            return appliedNewerEvents; 
        }

        /// <summary>
        /// If this object instance's ETag is the latest, this stores the sequence of events to the stream table and
        /// returns true. The method returns false if the current ETag is outdated. Does not update the snapshot.
        /// </summary>
        /// <param name="deltas">The list of domain events to store.</param>
        /// <param name="requireCurrentETag">When true, events will only store/apply if the model state is up to date. Some types of events are not sensitive to this
        /// (such as posting a deposit to an account), while others are (such as posting a withdrawl, which may cause an overdraft if approved against a stale
        /// model state).</param>
        internal async Task<bool> WriteEvents(IReadOnlyList<DomainEventBase> deltas, bool requireCurrentETag)
        {
            if (!IsInitialized) throw new Exception("The EventStream has not been initialized");

            using var connection = new SqlConnection(Config.Database.ConnectionString);
            await connection.OpenAsync();

            long maxETag = await ReadNewestETag(connection);

            // verify this object instance is up to date with stored events
            if (requireCurrentETag && ETag != maxETag) return false;

            // special case to inititalize the stream
            if(maxETag == 0)
            {
                await WriteEvent(connection, new StreamInitialized
                {
                    Id = Id,
                    ETag = 0
                });

                await WriteSnapshot(connection);
            }

            foreach(var delta in deltas)
            {
                ETag++;
                if(delta.ETag == DomainEventBase.EGAT_NOT_ASSIGNED)
                {
                    delta.ETag = ETag;
                    await WriteEvent(connection, delta);
                }
            }

            return true;
        }

        /// <summary>
        /// Updates this event stream's snapshot record with a new serialized model state. Does not apply any
        /// newer events that may have been stored to the event stream.
        /// </summary>
        internal async Task WriteSnapshot()
        {
            using var connection = new SqlConnection(Config.Database.ConnectionString);
            await connection.OpenAsync();
            await WriteSnapshot(connection);
            await connection.CloseAsync();
        }

        /// <summary>
        /// Returns this ID's current snapshot and the snapshot ETag, but does not update the values currently
        /// stored in this object instance.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task<(long ETag, TDomainModelRoot Snapshot)> ReadSnapshot(SqlConnection connection)
        {
            long etag = 0;
            var snapshot = Activator.CreateInstance<TDomainModelRoot>();
            snapshot.Id = Id;

            using var cmd = new SqlCommand($"SELECT [ETag], [Snapshot] FROM [{Config.Database.SnapshotTableName}] WHERE Id=@Id");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if(reader.HasRows)
            {
                await reader.ReadAsync();
                etag = reader.GetInt64(0);
                var serializedSnapshot = reader.GetString(1);
                snapshot = JsonConvert.DeserializeObject<TDomainModelRoot>(serializedSnapshot, JsonSettings);
            }
            await reader.CloseAsync();

            return (etag, snapshot);
        }

        /// <summary>
        /// Applies any newer events to the model state stored in this object instance. Does not update the snapshot.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task ApplyNewerEvents(SqlConnection connection)
        {
            using var cmd = new SqlCommand($"SELECT [ETag], [Payload] FROM [{Config.Database.LogTableName}] WHERE Id =@Id AND ETag > @ETag ORDER BY ETag ASC;");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@ETag", ETag);

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult);
            if(reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    ETag = reader.GetInt64(0);
                    var serializedDelta = reader.GetString(1);
                    var domainEvent = JsonConvert.DeserializeObject(serializedDelta, JsonSettings) as DomainEventBase;
                    ApplyMethods[domainEvent.GetType()].Invoke(State, new[] { domainEvent });
                }
            }
            await reader.CloseAsync();
        }

        /// <summary>
        /// Updates this event stream's snapshot record with a new serialized model state.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task WriteSnapshot(SqlConnection connection)
        {
            var serializedState = JsonConvert.SerializeObject(State, JsonSettings);

            using var cmd = new SqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = (ETag == 0)
                ? $"INSERT INTO [{Config.Database.SnapshotTableName}] ([Id], [ETag], [Snapshot]) VALUES (@Id, @ETag, @Snapshot);"
                : $"UPDATE [{Config.Database.SnapshotTableName}] SET [ETag]=@ETag, [Snapshot]=@Snapshot WHERE [Id]=@Id;";
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@ETag", ETag);
            cmd.Parameters.AddWithValue("@Snapshot", serializedState);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Returns the newest (highest) ETag number in the stream for this Id.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task<long> ReadNewestETag(SqlConnection connection)
        {
            // The MAX aggregate returns NULL for no rows, allowing ISNULL to substitute the
            // value 0, otherwise ExecuteScalarAsync would return null for an empty recordset
            using var cmd = new SqlCommand($"SELECT TOP 1 ISNULL(MAX([ETag]), 0) AS ETag FROM [{Config.Database.LogTableName}] WHERE [Id]=@Id ORDER BY [ETag] DESC;");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);
            var etag = (long)await cmd.ExecuteScalarAsync();
            return etag;
        }

        /// <summary>
        /// Writes a single domain event to the stream. Does not update the snapshot.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        /// <param name="delta">The domain event to store.</param>
        private async Task WriteEvent(SqlConnection connection, DomainEventBase delta)
        {
            var serializedDelta = JsonConvert.SerializeObject(delta, JsonSettings);
            using var cmd = new SqlCommand($"INSERT INTO [{Config.Database.LogTableName}] ([Id], [ETag], [Timestamp], [EventType], [Payload]) VALUES (@Id, @ETag, @Timestamp, @TypeName, @Payload);");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@ETag", delta.ETag);
            cmd.Parameters.AddWithValue("@Timestamp", delta.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@TypeName", delta.GetType().Name);
            cmd.Parameters.AddWithValue("@Payload", serializedDelta);
            await cmd.ExecuteNonQueryAsync();
        }

        internal JsonSerializerSettings JsonSettings
            = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
    }
}
