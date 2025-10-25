using System;
using System.Collections.Concurrent;
using System.Text;
using LogComponent;

namespace LogComponent.Tests
{
 public class LogWriterFake : ILogWriter
 {
 public ConcurrentQueue<string> Lines { get; } = new();
 public void Write(string text) => Lines.Enqueue(text);
 public void WriteLine(string text) => Lines.Enqueue(text + "\n");
 public void RotateIfNeeded(DateTime now) { /* no-op */ }
 public void Flush() { }
 public void Dispose() { }
 }
}
