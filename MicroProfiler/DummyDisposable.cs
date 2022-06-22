using System;

namespace MicroProfiler
{
    internal class DummyDisposable : IDisposable
    {
        public static IDisposable Instance { get; } = new DummyDisposable();
        private DummyDisposable() { }

        public void Dispose()
        {
            // Intentionally does nothing
        }
    }
}