
namespace LorePath.Stormsign
{ 
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The root interface for all events broadcast by the event bus.
    /// </summary>
    public interface IEventData { }
    
    public class EventBus : PSingleton<EventBus>
    {        
        // Dictionary to store subscribers for each event type
        private Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();
        private Dictionary<Type, List<Type>> _inheritance = new Dictionary<Type, List<Type>>();
        

        public static void Subscribe<T>(Action<T> handler) where T: IEventData => SetListener<T>(handler, true);

        public static void Unsubscribe<T>(Action<T> handler) where T: IEventData => SetListener<T>(handler, false);
        


        /// <summary>
        /// Subscribe the event channel of EventData Type T
        /// </summary>
        /// <typeparam name="T">The EventData Type of the channel to subsctibe to.</typeparam>
        /// <param name="handler">Method that will handle the event</param>
        public static void SetListener<T>(Action<T> handler, bool subscribe) where T : IEventData
        {
            if (_instance == null) return;
            _instance.SetListenerInternal<T>(handler, subscribe);
        }

        private void SetListenerInternal<T>(Action<T> handler, bool subscribe) where T : IEventData
        {
            Type eventType = typeof(T);

            if (!_subscribers.TryGetValue(eventType, out List<Delegate> handlers))
            {
                handlers = new List<Delegate>();
                _subscribers[eventType] = handlers;
                BuildInheritance(eventType);
            }

            if (subscribe && !handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
            else if (!subscribe) handlers.Remove(handler);
        }
        

        /// <summary>
        /// Raises an event of a type and all its base types.
        /// </summary>
        public static void RaiseEvent<T>(T eventData) where T : IEventData => _instance.RaiseEventInternal(eventData);

        private void RaiseEventInternal<T>(T eventData) where T : IEventData
        {
            Type eventType = typeof(T);
            List<Type> targetTypes = GetInheritance(eventType);
            foreach (var type in targetTypes)
            {
                if (_subscribers.TryGetValue(type, out List<Delegate> handlers))
                {
                    // Create a copy of handlers to avoid issues if a handler unsubscribes during event processing
                    List<Delegate> handlersCopy = new List<Delegate>(handlers);
                    foreach (Delegate handler in handlersCopy)
                    {
                        Action<T> typedHandler = (Action<T>)handler;
                        typedHandler(eventData);
                    }
                }
            }
        }
        

        /// <summary>
        /// Clear all subscribers for a specific EventData Type.
        /// </summary>
        /// <typeparam name="T">Event type that derives from EventData</typeparam>
        public static void ClearSubscribers<T>() where T : IEventData
        {
            Type eventType = typeof(T);
            if (Exists) _instance._subscribers.Remove(eventType);
        }
        
        /// <summary>
        /// Clear all subscribers for all EventData Types.
        /// </summary>
        public static void ClearAllSubscribers()
        {
            if (Exists) _instance._subscribers.Clear();
        }

        /// <summary>
        /// Builds an inheretance cache of types to broadcast to from the given type.
        /// </summary>        
        private static void BuildInheritance(Type type)
        {            
            if (!_instance._inheritance.TryGetValue(type, out List<Type> types))
            {
                types = new();
                for (var t = type; t != null && t != typeof(object); t = t.BaseType) types.Add(t);                
                foreach (var t in type.GetInterfaces()) types.Add(t);
                _instance._inheritance[type] = types;
            }
        }

        /// <summary>
        /// Gets the inheretance cache of types to broadcast to for a given type,
        /// building it if necessary.
        /// </summary>
        private static List<Type> GetInheritance(Type type)
        {
            if (!_instance._inheritance.TryGetValue(type, out List<Type> types))
            {
                types = new();
                for (var t = type; t != null && t != typeof(object); t = t.BaseType) types.Add(t);
                foreach (var t in type.GetInterfaces()) types.Add(t);
                _instance._inheritance[type] = types;
            }
            return types;
        }
    }
}