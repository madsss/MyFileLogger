using System;

namespace LogComponent
{
    public interface ILogWriter : IDisposable
    {
        void Write(string text);
        void WriteLine(string text);
        void RotateIfNeeded(DateTime now);
        void Flush();
    }
}
