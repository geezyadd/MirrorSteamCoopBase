using System;
using System.Collections.Generic;

namespace Features.MvpModule {
    public interface IGenericEventBus<in TBaseEvent> {
        void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : TBaseEvent;
        void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : TBaseEvent;
        void Publish<TEvent>(TEvent eventHandler) where TEvent : TBaseEvent;
        void StopPublish();
        void RestartPublish();
        void CleanUp();
    }

    public sealed class GenericEventBus<TBaseEvent> : IGenericEventBus<TBaseEvent> {
        private readonly Dictionary<Type, Delegate> _listeners = new();
        private bool _publishingStopped;

        public void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : TBaseEvent {
            Type eventType = typeof(TEvent);
            _listeners[eventType] = _listeners.TryGetValue(eventType, out Delegate existing)
                ? Delegate.Combine(existing, listener)
                : listener;
        }

        public void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : TBaseEvent {
            Type eventType = typeof(TEvent);
            if (_listeners.TryGetValue(eventType, out Delegate existing) == false)
                return;

            Delegate remaining = Delegate.Remove(existing, listener);
            if (remaining == null)
                _listeners.Remove(eventType);
            else
                _listeners[eventType] = remaining;
        }

        public void Publish<TEvent>(TEvent eventHandler) where TEvent : TBaseEvent {
            if (_publishingStopped)
                return;

            if (_listeners.TryGetValue(typeof(TEvent), out Delegate listener) == false)
                return;

            (listener as Action<TEvent>)?.Invoke(eventHandler);
        }

        public void StopPublish() =>
            _publishingStopped = true;

        public void RestartPublish() =>
            _publishingStopped = false;

        public void CleanUp() =>
            _listeners.Clear();
    }
}
