using System;
using System.Diagnostics;

namespace MicroProfiler
{
    internal class StepTracker<T> : IDisposable, IWithSequentialId where T : System.Enum
    {
        private Profiler<T> _currentProfiler;
        public TimeSpan StartTimepan { get; private set; }
        public T StepType { get; private set; }
        public int Index { get; private set; }
        public int Id { get; }

        public StepTracker(int id)
        {
            Id = id;   
        }

        public void StartFor(Profiler<T> profiler, TimeSpan startTimestamp, T stepType, int index)
        {
            _currentProfiler = profiler;
            StartTimepan = startTimestamp;
            StepType = stepType;
            Index = index;
        }

        public void Dispose()
        {
            _currentProfiler.ReturnFromStep(this);
            _currentProfiler = null;
        }
    }
}