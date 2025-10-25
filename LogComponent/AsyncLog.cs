using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace LogComponent
{
    public class AsyncLog : ILog, IDisposable
    {
        private readonly ConcurrentQueue<LogLine> _lines = new();
        private readonly ILogWriter _writer;
        private readonly IDateTimeProvider _dateTimeProvider;
        private Thread? _runThread;
        private DateTime _curDate;
        private const int DefaultBatchSize = 5;
        private readonly int _batchSize;
        private volatile bool _quitWithFlush = false;
        private volatile bool _exit = false;

        public AsyncLog(ILogWriter writer) : this(writer, new DefaultDateTimeProvider(), DefaultBatchSize)
        {
        }

        public AsyncLog(ILogWriter writer, IDateTimeProvider dateTimeProvider, int batchSize = DefaultBatchSize)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _batchSize = batchSize > 0 ? batchSize : DefaultBatchSize;
            _curDate = _dateTimeProvider.Now;
        }

        // Start background worker explicitly to make behavior controllable in tests
        public void Start()
        {
            if (_runThread != null) return;
            _runThread = new Thread(MainLoop) { IsBackground = true };
            _runThread.Start();
        }

        private void MainLoop()
        {
            try
            {
                var sb = new StringBuilder();

                while (!_exit)
                {
                    int processed = 0;

                    for (int i = 0; i < _batchSize; i++)
                    {
                        if (_exit && !_quitWithFlush)
                            break;

                        if (!_lines.TryDequeue(out var logLine))
                            break;

                        processed++;

                        try
                        {
                            var now = _dateTimeProvider.Now;
                            if (now.Date != _curDate.Date)
                            {
                                _curDate = now;
                                _writer.RotateIfNeeded(now);
                            }

                            sb.Clear();
                            sb.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                            sb.Append("\t");
                            sb.Append(logLine.LineText());
                            sb.Append("\t");
                            sb.Append(Environment.NewLine);

                            _writer.Write(sb.ToString());
                        }
                        catch (Exception ex)
                        {
                            try { Console.Error.WriteLine($"AsyncLog: unexpected error processing log line: {ex}"); } catch { }
                        }
                    }

                    if (_quitWithFlush && _lines.IsEmpty)
                    {
                        _exit = true;
                        break;
                    }

                    if (processed == 0)
                        Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                try { Console.Error.WriteLine($"AsyncLog: main loop fatal error: {ex}"); } catch { }
            }
            finally
            {
                try { _writer.Flush(); } catch { }
                try { _writer.Dispose(); } catch { }
            }
        }

        public void StopWithoutFlush()
        {
            _exit = true;
            try { _runThread?.Join(); } catch { }
        }

        public void StopWithFlush()
        {
            _quitWithFlush = true;
            try { _runThread?.Join(); } catch { }
        }

        public void Write(string text)
        {
            _lines.Enqueue(new LogLine() { Text = text, Timestamp = _dateTimeProvider.Now });
        }

        public void Dispose()
        {
            _exit = true;
            try { _runThread?.Join(500); } catch { }
            try { _writer?.Dispose(); } catch { }
        }
    }
}