
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventStreamDotNet
{
    /// <summary>
    /// Caches instances of domain event handlers, and caches and invokes all of their Apply methods. When used
    /// with dependency injection, register this as a Singleton scoped service. To use this without dependency
    /// injection, reference the instance provided by an <see cref="DirectDependencyServiceHost"/> object.
    /// </summary>
    public class DomainEventHandlerService
    {
        private readonly DebugLogger<DomainEventHandlerService> logger;

        // Keyed on the domain model root
        private Dictionary<Type, HandlerItem> handlers = new Dictionary<Type, HandlerItem>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configService">A collection of library configuration settings.</param>
        public DomainEventHandlerService(EventStreamConfigService configService)
        {
            logger = new DebugLogger<DomainEventHandlerService>(configService.LoggerFactory);
            logger.LogDebug($"{nameof(DomainEventHandlerService)} is starting");
        }

        /// <summary>
        /// Adds a new domain event handler to the cache.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the event handler.</typeparam>
        /// <param name="handler">An instance of the event handler (this will be stored).</param>
        public void RegisterDomainEventHandler<TDomainModelRoot, TDomainModelEventHandler>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
            where TDomainModelEventHandler : class, IDomainModelEventHandler<TDomainModelRoot>, new()
        {
            // ignore the registration if we've already seen this one
            var domainType = typeof(TDomainModelRoot);
            if (handlers.ContainsKey(domainType)) return;

            // create an instance of the handler
            var handler = Activator.CreateInstance(typeof(TDomainModelEventHandler)) as IDomainModelEventHandler<TDomainModelRoot>;
            var handlerType = handler.GetType();

            logger.LogDebug($"Registering domain event handler {handlerType.Name} for domain model {domainType.Name}");

            // prepare to store the handler
            var item = new HandlerItem { Handler = handler };

            // validate and catalog the Apply methods
            var methods = handlerType.GetMethods();
            foreach(var method in methods)
            {
                if(method.Name == "Apply")
                {
                    var parms = method.GetParameters();
                    if (parms.Length != 1) 
                        throw new ArgumentException($"Invalid Apply method in domain event handler {handlerType.Name}: methods require exactly one argument");

                    var type = parms[0].ParameterType;
                    if (!typeof(DomainEventBase).IsAssignableFrom(type)) 
                        throw new ArgumentException($"Invalid Apply method in domain event handler {handlerType.Name}: argument must derive from {nameof(DomainEventBase)}");
                    
                    item.ApplyMethods.Add(type, method);
                    logger.LogDebug($"  Caching Apply method for domain event {type.Name}");
                }
            }
            if (item.ApplyMethods.Count == 0) 
                throw new ArgumentException($"No Apply methods found for domain event handler {handlerType.Name}");

            // store the handler
            handlers.Add(domainType, item);
        }

        /// <summary>
        /// Indicates whether a domain event handler has been registered for the given domain model.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the event handler.</typeparam>
        public bool IsDomainEventHandlerRegistered<TDomainModelRoot>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
            => handlers.ContainsKey(typeof(TDomainModelRoot));

        /// <summary>
        /// Removes a domain event handler from the cache.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the event handler.</typeparam>
        public void UnregisterDomainEventHandler<TDomainModelRoot>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
        {
            var domainType = typeof(TDomainModelRoot);
            if (handlers.ContainsKey(domainType)) handlers.Remove(domainType);
        }

        /// <summary>
        /// Applies a domain event to the domain model state using a cached event handler.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model used with the event handler.</typeparam>
        /// <param name="modelState">The domain model state to be updated.</param>
        /// <param name="loggedEvent">The domain event to be applied.</param>
        internal void ApplyEvent<TDomainModelRoot>(TDomainModelRoot modelState, DomainEventBase loggedEvent)
            where TDomainModelRoot : class, IDomainModelRoot, new()
        {
            var domainType = typeof(TDomainModelRoot);
            if (!handlers.ContainsKey(domainType)) 
                throw new ArgumentException($"No domain event handler registered for domain model {domainType.Name}");

            var item = handlers[domainType];
            var handlerType = item.Handler.GetType();
            var eventType = loggedEvent.GetType();
            if (!item.ApplyMethods.ContainsKey(eventType)) 
                throw new ArgumentException($"Domain event handler {handlerType.Name} does not support domain event {eventType.Name}");

            var method = item.ApplyMethods[eventType];
            var handler = item.Handler as IDomainModelEventHandler<TDomainModelRoot>;

            handler.DomainModelState = modelState;
            method.Invoke(handler, new[] { loggedEvent });
            handler.DomainModelState = default; // null

            logger.LogDebug($"Domain event handler {handlerType.Name} applied event {eventType.Name} to {domainType.Name} model state");
        }

        // Associates a domain event handler instance with a list of its Apply methods
        private class HandlerItem
        {
            public object Handler;

            // Keyed on the domain event type, value is the Apply method to invoke
            public Dictionary<Type, MethodInfo> ApplyMethods = new Dictionary<Type, MethodInfo>();
        }
    }
}
