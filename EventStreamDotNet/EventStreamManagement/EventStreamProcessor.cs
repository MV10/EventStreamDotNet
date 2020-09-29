
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// Handles the domain model state, the event stream delta logging, and snapshot creation.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    internal class EventStreamProcessor<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot, new()
    {
        /// <summary>
        /// The unique ID assigned to this event stream.
        /// </summary>
        internal string Id;

        /// <summary>
        /// The entity tag (aka version number) for the domain model state stored in this object instance.
        /// </summary>
        internal long ETag = 0;

        /// <summary>
        /// Represents the ETag last read from the snapshot table. Can be used to decide if/when to
        /// output a new snapshot.
        /// </summary>
        internal long LastKnownShapshotETag = -1;

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
        private TDomainModelRoot domainModelState;

        private readonly DomainEventHandlerService eventHandlerService;

        /// <summary>
        /// During public/internal processing, a list of event stream and snapshot projection
        /// handlers are collected. Prior to exit, the handlers are invoked in order, then
        /// the list is reset.
        /// </summary>
        private List<Action<IDomainModelRoot>> queuedProjections = new List<Action<IDomainModelRoot>>();

        /// <summary>
        /// Ensures Type metadata is stored with the serialized output.
        /// </summary>
        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly DebugLogger<EventStreamProcessor<TDomainModelRoot>> logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configService">A collection of library configuration settings.</param>
        /// <param name="eventHandlerService">A collection of domain event handlers.</param>
        public EventStreamProcessor(EventStreamConfigService configService, DomainEventHandlerService eventHandlerService)
        {
            if (!configService.ContainsConfiguration<TDomainModelRoot>()) throw new Exception($"No configuration registered for domain model {typeof(TDomainModelRoot).Name}");

            this.eventHandlerService = eventHandlerService;

            IsInitialized = false;
            Config = configService.GetConfiguration<TDomainModelRoot>();
            logger = new DebugLogger<EventStreamProcessor<TDomainModelRoot>>(Config.LoggerFactory);

            logger.LogDebug($"Created {nameof(EventStreamProcessor<TDomainModelRoot>)} for domain model root {typeof(TDomainModelRoot).Name}");
        }


        // ---------- Internal methods invoked by EventStreamManager

        /// <summary>
        /// Reads the snapshot and all newer events. See <see cref="ReadAllEvents"/> for details
        /// of how the snapshot may be updated.
        /// </summary>
        /// <param name="id">The unique identifier for this domain model object.</param>
        internal async Task Initialize(string id)
        {
            logger.LogDebug($"{nameof(Initialize)} ID {id}");

            if (!eventHandlerService.IsDomainEventHandlerRegistered<TDomainModelRoot>())
                throw new Exception($"No domain event handler registered for domain model {typeof(TDomainModelRoot).Name}");

            Id = id;

            IsInitialized = true;

            await ReadAllEvents();
        }

        /// <summary>
        /// Returns a copy of the currently known state. Does not attempt to synchronize with newer
        /// events that may have been written to this stream by other clients.
        /// </summary>
        internal TDomainModelRoot CopyState()
        {
            logger.LogDebug($"{nameof(CopyState)} ID {Id}");

            if (!IsInitialized) throw new Exception("The EventStream has not been initialized");

            var json = JsonConvert.SerializeObject(domainModelState, jsonSettings);
            return JsonConvert.DeserializeObject<TDomainModelRoot>(json, jsonSettings);
        }

        /// <summary>
        /// Reads the snapshot, if any, then applies any newer events to the model. The snapshot is always
        /// updated by this call, if necessary, regardless of the configured snapshot update policy.
        /// The return value indicates whether the snapshot was updated.
        /// </summary>
        internal async Task<bool> ReadAllEvents()
        {
            logger.LogDebug($"{nameof(ReadAllEvents)} ID {Id}");

            if (!IsInitialized) throw new Exception("The EventStream has not been initialized");

            queuedProjections.Clear();

            using var connection = new SqlConnection(Config.Database.ConnectionString);
            await connection.OpenAsync();

            // get the latest snapshot
            (ETag, domainModelState) = await ReadSnapshot(connection);

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

            InvokeQueuedProjectionHandlers();

            // indicate whether a snapshot was updated
            return appliedNewerEvents; 
        }

        /// <summary>
        /// If this object instance's ETag is the latest, this stores the sequence of events to the stream table and
        /// returns true. The method returns false if the current ETag is outdated. The snapshot may be updated according
        /// to the configured policy.
        /// </summary>
        /// <param name="deltas">The list of domain events to store.</param>
        /// <param name="requireCurrentETag">When true, events will only store/apply if the model state is up to date. Some types of events are not sensitive to this
        /// (such as posting a deposit to an account), while others are (such as posting a withdrawl, which may cause an overdraft if approved against a stale
        /// model state).</param>
        internal async Task<bool> WriteEvents(IReadOnlyList<DomainEventBase> deltas, bool requireCurrentETag)
        {
            logger.LogDebug($"{nameof(WriteEvents)} ID {Id} for {deltas.Count} deltas");

            if (!IsInitialized) throw new Exception("The EventStream has not been initialized");

            queuedProjections.Clear();

            using var connection = new SqlConnection(Config.Database.ConnectionString);
            await connection.OpenAsync();

            long maxETag = await ReadNewestETag(connection);

            // verify this object instance is up to date with stored events
            if (requireCurrentETag && ETag != maxETag) return false;

            foreach(var delta in deltas)
            {
                ETag++;
                if(delta.ETag == DomainEventBase.ETAG_NOT_ASSIGNED)
                {
                    delta.Id = Id;
                    delta.ETag = ETag;
                    await WriteAndApplyEvent(connection, delta);
                }
            }

            if(Config.Policies.SnapshotFrequency == SnapshotFrequencyEnum.AfterAllEvents)
            {
                await WriteSnapshot();
            }

            InvokeQueuedProjectionHandlers();

            return true;
        }

        /// <summary>
        /// Updates this event stream's snapshot record with a new serialized model state. Does not apply any
        /// newer events that may have been stored to the event stream.
        /// </summary>
        internal async Task WriteSnapshot()
        {
            logger.LogDebug($"{nameof(WriteSnapshot)} ID {Id}");

            queuedProjections.Clear();

            using var connection = new SqlConnection(Config.Database.ConnectionString);
            await connection.OpenAsync();
            await WriteSnapshot(connection);
            await connection.CloseAsync();

            InvokeQueuedProjectionHandlers();
        }


        // ---------- Private intermediate processing used by the internal methods above

        /// <summary>
        /// Returns this ID's current snapshot and the snapshot ETag, but does not update the values currently stored
        /// in this object instance (except for ETag 0: initializing the stream creates a new data model root object).
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task<(long ETag, TDomainModelRoot Snapshot)> ReadSnapshot(SqlConnection connection)
        {
            long etag;
            TDomainModelRoot snapshot;

            using var cmd = new SqlCommand($"SELECT [ETag], [Snapshot] FROM [{Config.Database.SnapshotTableName}] WHERE Id=@Id");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if(reader.HasRows)
            {
                await reader.ReadAsync();
                etag = reader.GetInt64(0);
                var serializedSnapshot = reader.GetString(1);
                await reader.CloseAsync();

                logger.LogDebug($"{nameof(ReadSnapshot)} ID {Id} loaded ETag {etag}");

                snapshot = JsonConvert.DeserializeObject<TDomainModelRoot>(serializedSnapshot, jsonSettings);
            }
            else
            {
                await reader.CloseAsync();

                logger.LogDebug($"{nameof(ReadSnapshot)} ID {Id} not found, writing {nameof(StreamInitialized)} domain event");

                etag = 0;
                snapshot = Activator.CreateInstance<TDomainModelRoot>();
                snapshot.Id = Id;
                domainModelState = snapshot;

                await WriteAndApplyEvent(connection, new StreamInitialized
                {
                    Id = Id,
                    ETag = 0
                });

                await WriteSnapshot(connection);
            }

            LastKnownShapshotETag = etag;
            if (!LastSnapshotDuration.IsRunning) LastSnapshotDuration.Restart();

            return (etag, snapshot);
        }

        /// <summary>
        /// Applies any newer events to the model state stored in this object instance. Does not update the snapshot.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task ApplyNewerEvents(SqlConnection connection)
        {
            logger.LogDebug($"{nameof(ApplyNewerEvents)} ID {Id}");

            using var cmd = new SqlCommand($"SELECT [ETag], [Payload] FROM [{Config.Database.EventTableName}] WHERE Id =@Id AND ETag > @ETag ORDER BY ETag ASC;");
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
                    var domainEvent = JsonConvert.DeserializeObject(serializedDelta, jsonSettings) as DomainEventBase;
                    ApplyEvent(domainEvent);
                }
            }
            await reader.CloseAsync();
        }

        /// <summary>
        /// Returns the newest (highest) ETag number in the stream for this Id.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task<long> ReadNewestETag(SqlConnection connection)
        {
            // The MAX aggregate returns NULL for no rows, allowing ISNULL to substitute the
            // value 0, otherwise ExecuteScalarAsync would return null for an empty recordset
            using var cmd = new SqlCommand($"SELECT TOP 1 ISNULL(MAX([ETag]), 0) AS ETag FROM [{Config.Database.EventTableName}] WHERE [Id]=@Id ORDER BY [ETag] DESC;");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);
            var etag = (long)await cmd.ExecuteScalarAsync();

            logger.LogDebug($"{nameof(ReadNewestETag)} ID {Id} returning ETag {etag}");

            return etag;
        }

        /// <summary>
        /// If a domain event projection handler was defined for this event and the handler
        /// isn't already queued for invocation, add it.
        /// </summary>
        /// <param name="appliedEvent">The domain event that has been applied to the domain model state.</param>
        private void TryAddDomainEventProjectionHandlers(DomainEventBase appliedEvent)
        {
            var type = appliedEvent.GetType();
            var list = Config.ProjectionHandlers.DomainEventHandlers.Where(h => h.type.Equals(type)).ToList();
            foreach (var item in list)
            {
                if (!queuedProjections.Contains(item.handler))
                    queuedProjections.Add(item.handler);
            }
        }

        /// <summary>
        /// If snapshot projection handlers exist, add any to the queue for invocation which
        /// haven't already been enqueued.
        /// </summary>
        private void TryAddSnapshotProjectionHandlers()
        {
            foreach (var handler in Config.ProjectionHandlers.SnapshotHandlers)
            {
                if (!queuedProjections.Contains(handler))
                    queuedProjections.Add(handler);
            }
        }


        // ---------- The lowest level methods which directly mutate model and stream state

        /// <summary>
        /// Writes a single domain event to the stream and apply it to this object instance's copy
        /// of the domain model state. The snapshot may be updated according to the configured policy.
        /// <param name="connection">An open database connection.</param>
        /// <param name="delta">The domain event to store and apply.</param>
        private async Task WriteAndApplyEvent(SqlConnection connection, DomainEventBase delta)
        {
            var serializedDelta = JsonConvert.SerializeObject(delta, jsonSettings);
            var eventName = delta.GetType().Name;

            logger.LogDebug($"{nameof(WriteAndApplyEvent)} ID {Id} for domain event {eventName} with ETag {delta.ETag}");

            using var cmd = new SqlCommand($"INSERT INTO [{Config.Database.EventTableName}] ([Id], [ETag], [Timestamp], [EventType], [Payload]) VALUES (@Id, @ETag, @Timestamp, @TypeName, @Payload);");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@ETag", delta.ETag);
            cmd.Parameters.AddWithValue("@Timestamp", delta.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@TypeName", eventName);
            cmd.Parameters.AddWithValue("@Payload", serializedDelta);
            await cmd.ExecuteNonQueryAsync();

            // now that the event is part of the stream (eg. a logged, historical fact), apply it to our copy of the model
            ApplyEvent(delta);

            // process individual event-level snapshot policies
            switch (Config.Policies.SnapshotFrequency)
            {
                case SnapshotFrequencyEnum.AfterEachEvent:
                {
                    await WriteSnapshot(connection);
                    break;
                }

                case SnapshotFrequencyEnum.AfterIntervalDeltas:
                {
                    if (ETag - LastKnownShapshotETag >= Config.Policies.SnapshotInterval)
                    {
                        await WriteSnapshot(connection);
                    }
                    break;
                }

                case SnapshotFrequencyEnum.AfterIntervalSeconds:
                {
                    if (LastSnapshotDuration.ElapsedMilliseconds / 1000 >= Config.Policies.SnapshotInterval)
                    {
                        await WriteSnapshot(connection);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Applies a logged domain event to the current model state. Updates the instance ETag to match the event ETag.
        /// </summary>
        /// <param name="loggedEvent">The domain event to apply.</param>
        private void ApplyEvent(DomainEventBase loggedEvent)
        {
            eventHandlerService.ApplyEvent(domainModelState, loggedEvent);
            ETag = loggedEvent.ETag;
            TryAddDomainEventProjectionHandlers(loggedEvent);

            logger.LogDebug($"Applied event {loggedEvent.GetType().Name} to promote model ID {Id} to ETag {loggedEvent.ETag}");
        }

        /// <summary>
        /// Updates this event stream's snapshot record with a new serialized model state.
        /// </summary>
        /// <param name="connection">An open database connection.</param>
        private async Task WriteSnapshot(SqlConnection connection)
        {
            logger.LogDebug($"{nameof(WriteSnapshot)} ID {Id} ETag {ETag}");

            var serializedState = JsonConvert.SerializeObject(domainModelState, jsonSettings);

            using var cmd = new SqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = (ETag == 0)
                ? $"INSERT INTO [{Config.Database.SnapshotTableName}] ([Id], [ETag], [Snapshot]) VALUES (@Id, @ETag, @Snapshot);"
                : $"UPDATE [{Config.Database.SnapshotTableName}] SET [ETag]=@ETag, [Snapshot]=@Snapshot WHERE [Id]=@Id;";
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@ETag", ETag);
            cmd.Parameters.AddWithValue("@Snapshot", serializedState);
            await cmd.ExecuteNonQueryAsync();

            LastKnownShapshotETag = ETag;
            LastSnapshotDuration.Restart();

            TryAddSnapshotProjectionHandlers();
        }

        /// <summary>
        /// If any domain event or snapshot handlers were enqueued, invoke them now then clear
        /// the queue.
        /// </summary>
        private void InvokeQueuedProjectionHandlers()
        {
            logger.LogDebug($"{nameof(InvokeQueuedProjectionHandlers)} ID {Id}");

            var state = CopyState();

            foreach (var handler in queuedProjections)
                handler.Invoke(state);

            queuedProjections.Clear();
        }
    }
}
