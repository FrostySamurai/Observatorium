using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Samurai.Observatorium.Runtime
{
    public class EventSystem
    {
        #region Inner Types

        private class NoChannelFoundException : Exception
        {
            public NoChannelFoundException(Type type) : base($"No valid channel found for data type '{type.FullName}'.")
            {
            }
        }

        #endregion Inner Types
        
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
            return GetChannel<TData>().Register(callback);
        }

        public void Unregister<TData>(Action<TData> callback)
        {
            GetChannel<TData>().Unregister(callback);
        }

        public void Raise<TData>(TData data)
        {
            GetChannel<TData>().Raise(data);
        }

        #endregion General Events

        #region Keyed Events

        public IDisposable Register<TKey, TData>(Action<TData> callback, TKey key) where TData : IEventKeyProvider<TKey>
        {
            return GetChannel<TKey, TData>().Register(callback, key);
        }

        public void Unregister<TKey, TData>(Action<TData> callback, TKey key) where TData : IEventKeyProvider<TKey>
        {
            GetChannel<TKey, TData>().Unregister(callback, key);
        }

        #endregion Keyed Events

        #region Private

        private EventChannel<TData> GetChannel<TData>()
        {
            var channel = GetBaseChannel<TData>();
            if (channel is not EventChannel<TData> dataChannel)
            {
                throw new NoChannelFoundException(typeof(TData));
            }

            return dataChannel;
        }

        private EventChannel<TKey, TData> GetChannel<TKey, TData>() where TData : IEventKeyProvider<TKey>
        {
            var channel = GetBaseChannel<TData>();
            if (channel is not EventChannel<TKey, TData> dataChannel)
            {
                throw new NoChannelFoundException(typeof(TData));
            }

            return dataChannel;
        }

        private EventChannel GetBaseChannel<TData>()
        {
            var dataType = typeof(TData);
            if (!_channels.TryGetValue(dataType, out var channel))
            {
                throw new NoChannelFoundException(dataType);
            }

            return channel;
        }

        #endregion Private
    }
}