using System;

namespace Samurai.Observatorium.Runtime
{
    public interface IEventKeyProvider<out TKey> where TKey : IEquatable<TKey>
    {
        public TKey EventKey { get; }
    }
}