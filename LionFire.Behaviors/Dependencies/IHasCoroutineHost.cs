
namespace LionFire.Coroutines
{
//    public interface ICoroutineHostProvider
//    {
//        CoroutineHost GetCoroutineHost(object owner = null);
//    }
    public interface IHasCoroutineHost
    {
        CoroutineHost CoroutineHost { get; }
    }
}
