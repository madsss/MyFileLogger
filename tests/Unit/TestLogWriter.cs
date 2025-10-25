using System;
using System.Collections.Concurrent;
using System.Threading;
using LogComponent;

namespace LogComponent.UnitTests
{
    public class TestLogWriter : ILogWriter
    {
        public ConcurrentQueue<string> Lines { get; } = new();
        private ManualResetEventSlim _mre = new(false);
        private int _expected = 0;

        public void SetExpected(int expected)
        {
            _expected = expected;
            if (Lines.Count >= _expected)
            {
                _mre.Set();
            }
            else
            {
                _mre.Reset();
            }
        }

        public bool WaitForLines(int expected, int millisecondsTimeout = 2000)
        {
            SetExpected(expected);
            return _mre.Wait(millisecondsTimeout);
        }

        public void RotateIfNeeded(DateTime now) { }
        public void Write(string text)
        {
            Lines.Enqueue(text);
            if (Lines.Count >= _expected) _mre.Set();
        }

        public void WriteLine(string line)
        {
            Lines.Enqueue(line + "\n");
            if (Lines.Count >= _expected) _mre.Set();
        }
        public void Flush() { }
        public void Dispose() { try { _mre?.Dispose(); } catch { } }
    }
}
