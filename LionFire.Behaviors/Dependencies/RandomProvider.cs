#if NoDeps
using System;
using System.Threading;


namespace LionFire
{

    // Retrieved from http://csharpindepth.com/Articles/Chapter12/Random.aspx
    // as public domain.
    
    // SECURITY: Author warns that this is not secure.
    public static class RandomProvider
    {
        private static int seed = Environment.TickCount;

        private static ThreadLocal<Random> randomWrapper = new ThreadLocal<Random>(() =>
            new Random(Interlocked.Increment(ref seed))
        );

        public static Random ThreadRandom
        {
            get
            {
                return randomWrapper.Value;
            }
        }
    }
}
#endif