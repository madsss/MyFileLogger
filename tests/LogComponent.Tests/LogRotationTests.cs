using System;
using System.Linq;
using System.Threading;
using LogComponent;
using Xunit;

namespace LogComponent.Tests
{
 public class LogRotationTests
 {
 [Fact]
 public void CrossingMidnight_TriggersRotationAndWritesBothLines()
 {
 var provider = new MutableDateTimeProvider { Now = new DateTime(2025,1,1,23,59,58) };
 var fake = new RotationAwareWriter();

 using var log = new AsyncLog(fake, provider, batchSize:5);
 log.Start();

 fake.SetExpected(2);
 // write before midnight
 log.Write("line-before");

 // advance time to after midnight
 provider.Now = provider.Now.AddSeconds(3); // now next day

 // write after midnight
 log.Write("line-after");

 // deterministic wait
 Assert.True(fake.WaitForLines(2), "Expected two lines written within timeout");

 // request flush
 log.StopWithFlush();

 // both lines should have been written
 var written = fake.Lines.ToArray();
 Assert.Contains(written, w => w.Contains("line-before"));
 Assert.Contains(written, w => w.Contains("line-after"));

 // rotation should have been requested at least once
 Assert.True(fake.RotateCount >=1, "Expected at least one rotation when crossing midnight");
 }
 }

 internal class MutableDateTimeProvider : IDateTimeProvider
 {
 public DateTime Now { get; set; }
 }

 internal class RotationAwareWriter : ILogWriter
 {
 public readonly System.Collections.Concurrent.ConcurrentQueue<string> Lines = new();
 public int RotateCount { get; private set; } =0;
 private ManualResetEventSlim _mre = new(false);
 private int _expected =0;

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

 public bool WaitForLines(int expected, int millisecondsTimeout =2000)
 {
 SetExpected(expected);
 return _mre.Wait(millisecondsTimeout);
 }

 public void Write(string text)
 {
 Lines.Enqueue(text);
 if (Lines.Count >= _expected) _mre.Set();
 }

 public void WriteLine(string text)
 {
 Lines.Enqueue(text + "\n");
 if (Lines.Count >= _expected) _mre.Set();
 }

 public void RotateIfNeeded(DateTime now)
 {
 RotateCount++;
 }

 public void Flush() { }
 public void Dispose() { try { _mre?.Dispose(); } catch { } }
 }
}
