using System.Threading;
using LogComponent;
using Xunit;

namespace LogComponent.UnitTests
{
    public class AsyncLogUnitTests
    {
        [Fact]
        public void Write_IsFastAndEnqueues()
        {
            var writer = new TestLogWriter();
            var dt = new DefaultDateTimeProvider();
            using var log = new AsyncLog(writer, dt);
            log.Start();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            log.Write("quick");
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 50, "Write should be quick");

            // wait for processing (deterministic)
            Assert.True(writer.WaitForLines(1, 5000), "Expected one line to be written within timeout");
            log.StopWithFlush();
            Assert.True(writer.Lines.Count >= 1);
        }
    }
}
