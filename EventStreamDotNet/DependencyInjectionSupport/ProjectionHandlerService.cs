
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// Caches instances of projection handlers, and caches and invokes all of their handler methods. When used
    /// with dependency injection, register this as a Singleton scoped service. To use this without dependency
    /// injection, reference the instance provided by an <see cref="DirectDependencyServiceHost"/> object.
    /// </summary>
    public class ProjectionHandlerService
    {
        private readonly EventStreamConfigService configService;
        private readonly DebugLogger<ProjectionHandlerService> logger;

        // Keyed on the domain model root
        private Dictionary<Type, HandlerItem> handlers = new Dictionary<Type, HandlerItem>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configService">A collection of library configuration settings.</param>
        public ProjectionHandlerService(EventStreamConfigService configService)
        {
            this.configService = configService;
            logger = new DebugLogger<ProjectionHandlerService>(configService.LoggerFactory);
            logger.LogDebug($"{nameof(ProjectionHandlerService)} is starting");
        }

        /// <summary>
        /// Adds a new projection handler to the cache.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the projection handler.</typeparam>
        /// <param name="handler">An instance of the projection handler (this will be stored).</param>
        public void RegisterProjectionHandler<TDomainModelRoot, TDomainModelProjectionHandler>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
            where TDomainModelProjectionHandler : class, IDomainModelProjectionHandler<TDomainModelRoot>
        {
            // ignore the registration if we've already seen this one
            var domainType = typeof(TDomainModelRoot);
            if (handlers.ContainsKey(domainType)) return;

            // verify config exists for this domain model
            if(!configService.ContainsConfiguration<TDomainModelRoot>()) 
                throw new Exception($"No configuration registered for domain model {typeof(TDomainModelRoot).Name}");
            
            var projectionConfig = configService.GetConfiguration<TDomainModelRoot>().Projection;

            // create an instance of the handler
            var handler = Activator.CreateInstance(typeof(TDomainModelProjectionHandler), projectionConfig) as IDomainModelProjectionHandler<TDomainModelRoot>;
            var handlerType = handler.GetType();

            logger.LogDebug($"Registering projection handler {handlerType.Name} for domain model {domainType.Name}");

            // prepare to store the handler
            var item = new HandlerItem { Handler = handler };

            // validate and catalog the projection methods
            var methods = handlerType.GetMethods();
            foreach (var method in methods)
            {
                var snapshotAttr = (SnapshotProjectionAttribute)Attribute.GetCustomAttribute(method, typeof(SnapshotProjectionAttribute));
                var eventAttrs = (DomainEventProjectionAttribute[])Attribute.GetCustomAttributes(method, typeof(DomainEventProjectionAttribute));

                if(snapshotAttr != null || eventAttrs.Length > 0)
                {
                    if (method.GetParameters().Length != 0) 
                        throw new ArgumentException($"Projection handler {method.Name} is invalid: methods must not have arguments");

                    if (!typeof(Task).IsAssignableFrom(method.ReturnType)
                        || (AsyncStateMachineAttribute)method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) == null) 
                        throw new ArgumentException($"Projection handler {method.Name} is invalid: methods must return an async Task");

                    // store snapshot handler
                    if (snapshotAttr != null)
                    {
                        item.SnapshotMethods.Add(method);
                        logger.LogDebug($"  Caching projection method {method.Name} for snapshot updates");
                    }

                    // store domain event handlers
                    foreach(var domainEvent in eventAttrs)
                    {
                        if (item.EventMethods.Any(e => e.DomainEvent.Equals(domainEvent.DomainEvent) && e.ProjectionMethod.Equals(method))) 
                            throw new ArgumentException($"Projection handler method {method.Name} declares domain event {domainEvent.DomainEvent.Name} more than once");

                        var tuple = (domainEvent.DomainEvent, method);
                        item.EventMethods.Add(tuple);
                        logger.LogDebug($"  Caching projection method {method.Name} for domain event {domainEvent.DomainEvent.Name}");
                    }
                }
            }

            if (item.EventMethods.Count == 0 && item.SnapshotMethods.Count == 0) 
                throw new ArgumentException($"No projection handler methods found for domain event handler {handlerType.Name}");

            // store the handler
            handlers.Add(domainType, item);
        }

        /// <summary>
        /// Indicates whether a projection handler has been registered for the given domain model.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the projection handler.</typeparam>
        public bool IsProjectionHandlerRegistered<TDomainModelRoot>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
            => handlers.ContainsKey(typeof(TDomainModelRoot));

        /// <summary>
        /// Removes a projection handler from the cache.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the projection handler.</typeparam>
        public void UnregisterProjectionHandler<TDomainModelRoot>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
        {
            var domainType = typeof(TDomainModelRoot);
            if (handlers.ContainsKey(domainType)) handlers.Remove(domainType);
        }

        /// <summary>
        /// Invokes projection handler methods based on domain events and/or snapshot updates. Safe to call regardless
        /// of whether a handler is registered for the domain model (fails silently because projections are optional, 
        /// yet this method will be called frequently).
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the event handler.</typeparam>
        /// <param name="modelState">The domain model state to reference.</param>
        /// <param name="loggedEvents">The domain events which may trigger projections.</param>
        /// <param name="snapshotUpdated">When true, snapshot projections will be invoked.</param>
        internal async Task InvokeProjections<TDomainModelRoot>(TDomainModelRoot modelState, List<DomainEventBase> loggedEvents, bool snapshotUpdated)
            where TDomainModelRoot : class, IDomainModelRoot, new()
        {
            // fail silently if no handlers registered
            var domainType = typeof(TDomainModelRoot);
            if (!handlers.ContainsKey(domainType)) return;
            var item = handlers[domainType];

            List<MethodInfo> methodsToInvoke = new List<MethodInfo>();

            // we need the Types of the logged events; HashSet.Contains is very fast
            HashSet<Type> eventTypes = new HashSet<Type>(loggedEvents.Count + 1); // +1 in case it's zero-length
            foreach (var loggedEvent in loggedEvents)
                eventTypes.Add(loggedEvent.GetType());

            // collect domain event projections
            foreach (var projection in item.EventMethods)
            {
                if (!methodsToInvoke.Contains(projection.ProjectionMethod) && eventTypes.Contains(projection.DomainEvent))
                    methodsToInvoke.Add(projection.ProjectionMethod);
            }

            // collect snapshot projections
            if (snapshotUpdated)
            {
                foreach(var projection in item.SnapshotMethods)
                {
                    if (!methodsToInvoke.Contains(projection))
                        methodsToInvoke.Add(projection);
                }
            }

            if (methodsToInvoke.Count == 0) return;

            // async invocation of projection methods
            var handler = item.Handler as IDomainModelProjectionHandler<TDomainModelRoot>;
            handler.DomainModelState = modelState;
            var tasks = new List<Task>(methodsToInvoke.Count);
            foreach (var method in methodsToInvoke)
                tasks.Add(method.Invoke(handler, null) as Task);
            await Task.WhenAll(tasks);
            handler.DomainModelState = default; // null
        }

        // Associates a domain event handler instance with a list of its Apply methods
        private class HandlerItem
        {
            public object Handler;

            // List instead of a dictionary because the domain event "key" can be referenced by multiple methods in the same handler
            public List<(Type DomainEvent, MethodInfo ProjectionMethod)> EventMethods = new List<(Type DomainEvent, MethodInfo ProjectionMethod)>();

            // Value is the projection method to invoke
            public List<MethodInfo> SnapshotMethods = new List<MethodInfo>();
        }
    }
}
