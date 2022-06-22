namespace MicroProfiler
{
    public class ExpandingArray<T>
    {
        private T[] _objects;
        public T[] Array => _objects;
        public int Length => _objects.Length;
        private object _arrayExpansionLock = new object();
        public ExpandingArray(int startSize)
        {
            _objects = new T[startSize];
        }
        public T this[int i]
        {
            get
            {
                return _objects[i];
            }
            set
            {
                var l = _objects.Length;
                if(i >= l)
                    ExpandArray(l, i);
                _objects[i] = value;
            }
        }

        private void ExpandArray(int oldSize, int targetIndex)
        {
            lock(_arrayExpansionLock) 
            {
                if(_objects.Length == oldSize || targetIndex >= _objects.Length)
                {
                    var newSize = _objects.Length * 2;
                    while(targetIndex >= newSize)
                        newSize *= 2;
                    var newArray = new T[newSize];
                    for(int i = 0; i < _objects.Length; ++i)
                        newArray[i] = _objects[i];

                    _objects = newArray;
                }
            }
        }
    }
}