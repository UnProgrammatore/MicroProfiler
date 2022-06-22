using System;

namespace MicroProfiler
{
    public interface IProfiler<T> : IDisposable where T : System.Enum
    {
        IDisposable Step(T stepType, ProfilingLevel profilingLevel = ProfilingLevel.Standard);
        (ProfiledStep<T>[] Steps, int Size) GetData();
    }
}
