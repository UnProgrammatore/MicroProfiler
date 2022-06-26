using System.Collections.Generic;
using System.Threading;

namespace MicroProfiler
{
    internal static class TypedObject<T> where T : System.Enum 
    {
        private static readonly ReusableObjectsDispenser<StepTracker<T>> _stepTrackers = new ReusableObjectsDispenser<StepTracker<T>>(id => new StepTracker<T>(id));
        internal static StepTracker<T> GetOneStepTracker()
        {
            return _stepTrackers.GetOne();
        }

        internal static void ReleaseStepTracker(StepTracker<T> stepTracker)
        {
            _stepTrackers.Release(stepTracker);
        }

        private static readonly ReusableObjectsDispenser<ProfiledStep<T>> _profiledSteps = new ReusableObjectsDispenser<ProfiledStep<T>>(id => new ProfiledStep<T>(id));
        internal static ProfiledStep<T> GetOneProfiledStep()
        {
            return _profiledSteps.GetOne();
        }

        internal static void ReleaseProfiledSteps(ProfiledStep<T>[] steps, int howMany)
        {
            for(int i = 0; i < howMany; ++i)
                if(steps[i] != null)
                    _profiledSteps.Release(steps[i]);
        }

        private static readonly ReusableObjectsDispenser<Profiler<T>> _profilers = new ReusableObjectsDispenser<Profiler<T>>(id => new Profiler<T>(id));

        internal static Profiler<T> GetOneProfiler()
        {
            return _profilers.GetOne();
        }

        internal static void ReleaseProfiler(Profiler<T> profiler) 
        {
            _profilers.Release(profiler);
        }

        internal static readonly AsyncLocal<IProfiler<T>> AsyncLocalProfiler = new AsyncLocal<IProfiler<T>>();
    }
}