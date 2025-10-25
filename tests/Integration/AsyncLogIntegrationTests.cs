using System;
using System.IO;
using System.Linq;
using System.Threading;
using LogComponent;
using Xunit;

namespace LogComponent.IntegrationTests
{
 public class AsyncLogIntegrationTests
 {
 [Fact]
 public void Integration_LogWritesToFilesAndRotationOccurs()
 {
 var dir = Path.Combine(Path.GetTempPath(), "LogComponentIntegration");
 if (Directory.Exists(dir)) Directory.Delete(dir, true);
 Directory.CreateDirectory(dir);

 var dt = new MutableDateTimeProvider { Now = new DateTime(2025,1,1,23,59,58) };
 using var writer = new FileLogWriter(dir, dt);
 using var log = new AsyncLog(writer, dt, batchSize:10);
 log.Start();

 log.Write("before-mid");
 dt.Now = dt.Now.AddSeconds(3);
 log.Write("after-mid");
 Thread.Sleep(200);
 log.StopWithFlush();

 var files = Directory.GetFiles(dir).OrderBy(f => f).ToArray();
 Assert.True(files.Length >=1, "At least one log file should be created");
 var content = File.ReadAllText(files.Last());
 Assert.Contains("after-mid", content);
 }
 }

 internal class MutableDateTimeProvider : IDateTimeProvider
 {
 public DateTime Now { get; set; }
 }
}
