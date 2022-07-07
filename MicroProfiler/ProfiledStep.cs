using System;

namespace MicroProfiler
{
    public class ProfiledStep<T> : IWithSequentialId
    {
        private int _id;
        int IWithSequentialId.Id => _id;
        public ProfiledStep(int id)
        {
            _id = id;
        }

        public T StepType { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public int WithinIndex { get; private set; }

        public void SetTo(T stepType, TimeSpan startTime, TimeSpan endTime)
        {
            StepType = stepType;
            StartTime = startTime;
            EndTime = endTime;
        }
        public void SetWithin(int index)
        {
            WithinIndex = index;
        }
    }
}