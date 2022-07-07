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
        private AsyncLocal<int> _withinIndex = new AsyncLocal<int>();

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
            _withinIndex.Value = -1;
        }

        public IDisposable Step(T stepType, ProfilingLevel profilingLevel = ProfilingLevel.Standard)
        {
            if(_profilingLevel > profilingLevel)
                return DummyDisposable.Instance;
            var stepTracker = TypedObject<T>.GetOneStepTracker();
            var newIndex = Interlocked.Increment(ref _currentIndex);
            stepTracker.StartFor(this, _stopwatch.Elapsed, stepType, newIndex);
            var step = TypedObject<T>.GetOneProfiledStep();
            _profiledSteps[newIndex] = step;
            step.SetWithin(_withinIndex.Value);
            _withinIndex.Value = newIndex;
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

        public void ReturnFromStep(StepTracker<T> stepTracker) 
        {
            var endTime = _stopwatch.Elapsed;
            var step = _profiledSteps[stepTracker.Index];
            step.SetTo(stepTracker.StepType, stepTracker.StartTimepan, endTime);
            _withinIndex.Value = step.WithinIndex;
        }
    }
}
