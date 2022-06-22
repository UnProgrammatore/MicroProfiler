using System;
using System.Threading;

namespace MicroProfiler
{
    internal class ReusableObjectsDispenser<T> where T : IWithSequentialId
    {
        // TODO implement a better version of this class which is able to more reliably get an available object, maybe by keeping a list of objects that are probably gonna be available
        private readonly Func<int, T> _buildOne;
        private int _currentIndex = -1;

        private ExpandingArray<LockableObject> _objects;
        private object _arrayExpansionLock = new object();
        private Random _random = new Random();
        public ReusableObjectsDispenser(Func<int, T> buildOne)
        {
            _buildOne = buildOne;
            _objects = new ExpandingArray<LockableObject>(16);
        }

        private class LockableObject 
        {
            public LockableObject(T value, SemaphoreSlim semaphore)
            {
                Value = value;
                Semaphore = semaphore;
            }

            public T Value { get; }
            public SemaphoreSlim Semaphore { get; }
        }

        public T GetOne() 
        {
            if(_currentIndex >= 0)
            {
                // The random starting index avoids having an uneven probability of each object being in use when looping through them
                var startFrom = _random.Next(_currentIndex + 1);
                var currentLength = _objects.Length;
                for(int i = 0; i < currentLength; ++i)
                {
                    var actualIndex = (i + startFrom) % currentLength;
                    var candidate = _objects[actualIndex];
                    if(candidate != null && candidate.Semaphore.Wait(0)) // Only gets the object if it's immediately free
                    {
                        return candidate.Value;
                    }
                }
            }
            return MakeNewAndLock();
        }

        private T MakeNewAndLock() 
        {
            var newIndex = Interlocked.Increment(ref _currentIndex);
            var newValue = _buildOne(newIndex);
            if(newValue.Id != newIndex)
                throw new ArgumentException($"The provided {nameof(_buildOne)} function returned an object with the wrong id");
            var newLockableObject = new LockableObject(newValue, new SemaphoreSlim(1));
            newLockableObject.Semaphore.Wait(0); // This will always return true because nothing else even know about this object yet
            _objects[newIndex] = newLockableObject;
            return newLockableObject.Value;
        }

        public void Release(T toRelease)
        {
            _objects[toRelease.Id].Semaphore.Release();
        }
    }
}