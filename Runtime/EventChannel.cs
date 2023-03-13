using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace Samurai.Observatorium.Runtime
{
    public abstract class EventChannel
    {
        #region Inner Types

        protected class CallbackDisposer : IDisposable
        {
            private Action _onDispose;
            
            public CallbackDisposer(Action onDispose)
            {
                _onDispose = onDispose;
            }
            
            public void Dispose()
            {
                _onDispose?.Invoke();
            }
        }

        #endregion Inner Types
        
        public abstract Type DataType { get; }
    }
    
    public abstract class EventChannel<TData> : EventChannel
    {
        private List<Action<TData>> _callbacks = new();

        public override Type DataType => typeof(TData);

        public IDisposable Register(Action<TData> callback)
        {
            _callbacks.Add(callback);
            
            return new CallbackDisposer(() => Unregister(callback));
        }

        public virtual void Unregister(Action<TData> callback)
        {
            _callbacks.Remove(callback);
        }

        public virtual void Raise(TData data)
        {
            _callbacks.ForEach(x => x?.Invoke(data));
        }
    }

    public abstract class EventChannel<TKey, TData> : EventChannel<TData> where TData : IEventKeyProvider<TKey>
    {
        private Dictionary<TKey, List<Action<TData>>> _mappedCallbacks = new();

        public IDisposable Register(Action<TData> callback, TKey key)
        {
            GetCallbacks(key).Add(callback);

            return new CallbackDisposer(() => Unregister(callback, key));
        }

        public void Unregister(Action<TData> callback, TKey key)
        {
            GetCallbacks(key).Remove(callback);
        }

        public override void Unregister(Action<TData> callback)
        {
            bool found = false;
            TKey key = default;
            foreach (var entry in _mappedCallbacks)
            {
                if (entry.Value.Any(x => x == callback))
                {
                    key = entry.Key;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                _mappedCallbacks[key].Remove(callback);
                return;
            }
            
            base.Unregister(callback);
        }

        public override void Raise(TData data)
        {
            if (data is IEventKeyProvider<TKey> keyedData)
            {
                GetCallbacks(keyedData.EventKey).ForEach(x => x?.Invoke(data));
            }
            
            base.Raise(data);
        }

        private List<Action<TData>> GetCallbacks(TKey key)
        {
            if (_mappedCallbacks.TryGetValue(key, out var callbacks))
            {
                return callbacks;
            }

            callbacks = ListPool<Action<TData>>.Get();
            _mappedCallbacks[key] = callbacks;
            return callbacks;
        }
    }
}