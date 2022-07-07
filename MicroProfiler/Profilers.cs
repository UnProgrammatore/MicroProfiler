using System;

namespace MicroProfiler
{
    public static class Profilers
    {
        public static IProfiler<T> StartNew<T>() where T : System.Enum 
        {
            var profiler = TypedObject<T>.GetOneProfiler();
            SetCurrentProfiler(profiler);
            profiler.Start();
            return profiler;
        }

        public static IProfiler<T> Current<T>() where T : System.Enum
            => TypedObject<T>.AsyncLocalProfiler.Value;

        public static void SetCurrentProfiler<T>(IProfiler<T> profiler) where T : System.Enum
        {
            TypedObject<T>.AsyncLocalProfiler.Value = profiler;
        }
    }
}
