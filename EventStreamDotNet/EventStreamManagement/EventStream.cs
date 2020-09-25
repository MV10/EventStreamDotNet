
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    internal class EventStream<TDomainModelRoot, TDomainEventHandler>
        where TDomainModelRoot : class, IDomainModelRoot
        where TDomainEventHandler : class, IDomainModelEventHandler<TDomainModelRoot>, new()
    {
        internal readonly string Id;

        internal long ETag;

        internal TDomainModelRoot State;

        internal readonly EventStreamDotNetConfig Config;

        internal readonly IDomainModelEventHandler<TDomainModelRoot> EventHandler;

        internal bool IsInitialized;

        internal EventStream(string id, EventStreamDotNetConfig config)
        {
            Id = id;
            Config = config;
            EventHandler = Activator.CreateInstance<TDomainEventHandler>();
            IsInitialized = false;
        }

        internal void Initialize()
        {
            IsInitialized = true;
        }

        // ReadStateFromStorage         ReadAllEvents           -- reads snapshot and all newer events
        // ApplyUpdatesToStorage        WriteEvents             -- updates the event stream
        // ReadSnapshot                 ReadSnapshot            -- only reads the current snapshot
        // ApplyNewerEvents             ApplyNewEvents          -- updates a snapshot in memory, optionally writes
        // WriteSnapshot                WriteSnapshot           -- output model state as a snapshot
        // GetEventStreamVersion        ReadETag                -- retrieves higest stored ETag
        // WriteEvent                   WriteEvent              -- writes a single event

        private JsonSerializerSettings JsonSettings
            = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
    }
}
