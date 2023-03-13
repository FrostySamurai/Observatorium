using System;
using UnityEngine;

namespace Samurai.Observatorium.Runtime
{
    public abstract class ScriptableEventChannel : ScriptableObject
    {
        public abstract EventChannel Channel { get; }
        public abstract Type DataType { get; }
    }
    
    public abstract class ScriptableEventChannel<TData> : ScriptableEventChannel
    {
        private class EventChannelWrapper : EventChannel<TData> {}

        private readonly EventChannelWrapper _channel = new();
        
        public override EventChannel Channel => _channel;
        public override Type DataType => typeof(TData);
    }
    
    public abstract class ScriptableEventChannel<TKey, TData> : ScriptableEventChannel where TData : IEventKeyProvider<TKey>
    {
        private class EventChannelWrapper : EventChannel<TKey, TData> {}

        private readonly EventChannelWrapper _channel = new();
        
        public override EventChannel Channel => _channel;
        public override Type DataType => typeof(TData);
    }
}