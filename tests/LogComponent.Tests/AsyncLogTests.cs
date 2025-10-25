using System.Linq;
using System.Threading;
using LogComponent;
using Xunit;

namespace LogComponent.Tests
{
    public class AsyncLogTests
    {
        [Fact]
        public void WritesLinesToWriter()
        {
            var writer = new TestLogWriter();
            using var log = new AsyncLog(writer);
            log.Start();

            writer.SetExpected(2);
            log.Write("hello");
            log.Write("world");

            // deterministic wait
            Assert.True(writer.WaitForLines(2), "Expected2 lines written within timeout");

            // request stop with flush and wait
            log.StopWithFlush();

            Assert.True(writer.Lines.Count >= 2);
            var all = writer.Lines.ToArray();
            Assert.Contains(all, l => l.Contains("hello"));
            Assert.Contains(all, l => l.Contains("world"));
        }

        [Fact]
        public void StopWithoutFlush_DiscardsPending_Deterministic()
        {
            var writer = new TestLogWriter();
            using var log = new AsyncLog(writer);
            log.Start();

            // enqueue many items
            int total = 1000;
            int expectedProcessed = 50; // we expect some to be processed
            writer.SetExpected(expectedProcessed);
            for (int i = 0; i < total; i++) log.Write($"m{i}");

            // wait until some lines are written
            Assert.True(writer.WaitForLines(expectedProcessed, 5000));

            // now stop without flush immediately
            log.StopWithoutFlush();

            // give a small grace (not sleep for long) to let join finish
            // assert that not all items were written
            Assert.True(writer.Lines.Count < total);
        }
    }
}
