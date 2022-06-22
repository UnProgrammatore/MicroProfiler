using System;
using System.Diagnostics;

namespace MicroProfiler
{
    internal class StepTracker<T> : IDisposable, IWithSequentialId where T : System.Enum
    {
        private Profiler<T> _currentProfiler;
        private TimeSpan _startTimepan;
        private T _stepType;
        public int Id { get; }

        public StepTracker(int id)
        {
            Id = id;   
        }

        public void StartFor(Profiler<T> profiler, TimeSpan startTimestamp, T stepType)
        {
            _currentProfiler = profiler;
            _startTimepan = startTimestamp;
            _stepType = stepType;
        }

        public void Dispose()
        {
            _currentProfiler.ReturnFromStep(_startTimepan, _stepType);
            _currentProfiler = null;
        }
    }
}