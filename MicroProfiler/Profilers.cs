using System;

namespace MicroProfiler
{
    public static class Profilers
    {
        public static IProfiler<T> StartNew<T>() where T : System.Enum 
        {
            var profiler = TypedObject<T>.GetOneProfiler();
            profiler.Start();
            return profiler;
        }
    }
}
