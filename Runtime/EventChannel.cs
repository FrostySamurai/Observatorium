using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            if (_callbacks.Contains(callback))
            {
                return new CallbackDisposer(null);
            }
            
            _callbacks.Add(callback);
            return new CallbackDisposer(() => Unregister(callback));
        }

        public virtual void Unregister(Action<TData> callback)
        {
            _callbacks.Remove(callback);
        }

        public virtual void Raise(TData data)
        {
            _callbacks.ForEach(x => Raise(ref x, ref data));
        }

        protected void Raise(ref Action<TData> callback, ref TData data)
        {
            try
            {
                callback?.Invoke(data);
            }
            catch (Exception e)
            {
                if (callback == null)
                {
                    Debug.LogError($"Raising an event failed! Callback is null..");
                    return;
                }
                
                Debug.LogError($"Raising an event failed! Data: {data} | Callback: {callback?.Target} - {callback?.Method}{Environment.NewLine}{e.Message}{Environment.NewLine}{e.StackTrace}");
            }
        }
    }

    public abstract class EventChannel<TKey, TData> : EventChannel<TData> where TData : IEventKeyProvider<TKey> where TKey : IEquatable<TKey>
    {
        private Dictionary<TKey, List<Action<TData>>> _mappedCallbacks = new();

        public IDisposable Register(TKey key, Action<TData> callback)
        {
            var callbacks = GetCallbacks(key);
            if (callbacks.Contains(callback))
            {
                return new CallbackDisposer(null);
            }

            callbacks.Add(callback);
            return new CallbackDisposer(() => Unregister(key, callback));
        }

        public void Unregister(TKey key, Action<TData> callback)
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
            // TODO: try catch when raising
            if (data is IEventKeyProvider<TKey> keyedData)
            {
                GetCallbacks(keyedData.EventKey).ForEach(x => Raise(ref x, ref data));
            }
            
            base.Raise(data);
        }

        private List<Action<TData>> GetCallbacks(TKey key)
        {
            if (_mappedCallbacks.TryGetValue(key, out var callbacks))
            {
                return callbacks;
            }

            callbacks = new List<Action<TData>>();
            _mappedCallbacks[key] = callbacks;
            return callbacks;
        }
    }
}