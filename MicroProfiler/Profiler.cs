using System;
using System.Diagnostics;
using System.Threading;

namespace MicroProfiler
{
    internal class Profiler<T> : IProfiler<T>, IWithSequentialId where T : System.Enum
    {
        private readonly ExpandingArray<ProfiledStep<T>> _profiledSteps = new ExpandingArray<ProfiledStep<T>>(16);
        private int _currentIndex = -1; 
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly int _id;
        int IWithSequentialId.Id => _id;

        private ProfilingLevel _profilingLevel;

        public Profiler(int id)
        {
            _id = id;
        }

        internal void Start(ProfilingLevel profilingLevel = ProfilingLevel.Standard) 
        {
            _stopwatch.Restart();
            _currentIndex = -1;
            _profilingLevel = profilingLevel;
        }

        public IDisposable Step(T stepType, ProfilingLevel profilingLevel = ProfilingLevel.Standard)
        {
            if(_profilingLevel > profilingLevel)
                return DummyDisposable.Instance;
            var stepTracker = TypedObject<T>.GetOneStepTracker();
            stepTracker.StartFor(this, _stopwatch.Elapsed, stepType);
            return stepTracker;
        }
        public void Dispose()
        {
            _stopwatch.Stop();
            TypedObject<T>.ReleaseProfiledSteps(_profiledSteps.Array, _currentIndex + 1);
            TypedObject<T>.ReleaseProfiler(this);
        }
        public (ProfiledStep<T>[] Steps, int Size) GetData()
        {
            return (_profiledSteps.Array, _currentIndex + 1);
        }

        public void ReturnFromStep(TimeSpan startTime, T stepType) 
        {
            var endTime = _stopwatch.Elapsed;
            var step = TypedObject<T>.GetOneProfiledStep();
            step.SetTo(stepType, startTime, endTime);
            var newIndex = Interlocked.Increment(ref _currentIndex);
            _profiledSteps[newIndex] = step;
        }
    }
}
