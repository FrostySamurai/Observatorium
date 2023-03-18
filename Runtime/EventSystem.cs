using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Samurai.Observatorium.Runtime
{
    public class EventSystem
    {
        private Dictionary<Type, EventChannel> _channels = new();

        #region Lifecycle

        public EventSystem(IEnumerable<Assembly> assemblies)
        {
            LoadFromAssemblies(assemblies);
        }

        public EventSystem(string channelsFolder)
        {
            LoadFromResources(channelsFolder);
        }

        public EventSystem(IEnumerable<Assembly> assemblies, string channelsFolder)
        {
            LoadFromAssemblies(assemblies);
            LoadFromResources(channelsFolder);
        }

        private void LoadFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            var baseType = typeof(EventChannel);
            foreach (var assembly in assemblies)
            {
                var channelTypes = assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && !x.IsAbstract && !x.IsNestedPrivate);
                foreach (var type in channelTypes)
                {
                    var instance = Activator.CreateInstance(type) as EventChannel;
                    _channels[instance.DataType] = instance;
                }
            }
        }

        private void LoadFromResources(string channelsFolder)
        {
            if (string.IsNullOrEmpty(channelsFolder))
            {
                return;
            }

            var channels = Resources.LoadAll<ScriptableEventChannel>(channelsFolder);
            foreach (var channel in channels)
            {
                _channels[channel.DataType] = channel.Channel;
            }
        }

        #endregion Lifecycle

        #region General Events

        public IDisposable Register<TData>(Action<TData> callback)
        {
            return GetChannel<TData>()?.Register(callback);
        }

        public void Unregister<TData>(Action<TData> callback)
        {
            GetChannel<TData>()?.Unregister(callback);
        }

        public void Raise<TData>(TData data)
        {
            GetChannel<TData>()?.Raise(data);
        }

        public void Raise<TKey, TData>(TData data) where TData : IEventKeyProvider<TKey> where TKey : IEquatable<TKey>
        {
            GetChannel<TKey, TData>()?.RaiseKeyed(data);
        }

        #endregion General Events

        #region Keyed Events

        public IDisposable Register<TKey, TData>(TKey key, Action<TData> callback) where TData : IEventKeyProvider<TKey> where TKey : IEquatable<TKey>
        {
            return GetChannel<TKey, TData>()?.Register(key, callback);
        }

        public void Unregister<TKey, TData>(TKey key, Action<TData> callback) where TData : IEventKeyProvider<TKey> where TKey : IEquatable<TKey>
        {
            GetChannel<TKey, TData>()?.Unregister(key, callback);
        }

        #endregion Keyed Events

        #region Private

        private EventChannel<TData> GetChannel<TData>()
        {
            if (!TryGetBaseChannel<TData>(out var channel))
            {
                return null;
            }
            
            if (channel is not EventChannel<TData> dataChannel)
            {
                LogChannelNotFound(typeof(TData));
                return null;
            }

            return dataChannel;
        }

        private EventChannel<TKey, TData> GetChannel<TKey, TData>() where TData : IEventKeyProvider<TKey> where TKey : IEquatable<TKey>
        {
            if (!TryGetBaseChannel<TData>(out var channel))
            {
                return null;
            }
            
            if (channel is not EventChannel<TKey, TData> dataChannel)
            {
                LogChannelNotFound(typeof(TData));
                return null;
            }

            return dataChannel;
        }

        private bool TryGetBaseChannel<TData>(out EventChannel channel)
        {
            var dataType = typeof(TData);
            if (!_channels.TryGetValue(dataType, out channel))
            {
                LogChannelNotFound(dataType);
                return false;
            }

            return true;
        }

        private void LogChannelNotFound(Type dataType)
        {
            Debug.LogError($"[EventSystem] There was no event channel found for data type '{dataType.FullName}'. " +
                           $"Make sure there is a class inheriting from EventChannel<> or ScriptableEventChannel<>.");
        }

        #endregion Private
    }
}