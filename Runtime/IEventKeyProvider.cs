namespace Samurai.Observatorium.Runtime
{
    public interface IEventKeyProvider<out TKey>
    {
        public TKey EventKey { get; }
    }
}