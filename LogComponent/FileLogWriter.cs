using System;
using System.IO;

namespace LogComponent
{
 public class FileLogWriter : ILogWriter
 {
 private readonly string _directory;
 private StreamWriter? _writer;
 private DateTime _currentDate;
 private readonly IDateTimeProvider _dateTimeProvider;

 public FileLogWriter(string directory, IDateTimeProvider? dateTimeProvider = null)
 {
 _directory = directory ?? throw new ArgumentNullException(nameof(directory));
 _dateTimeProvider = dateTimeProvider ?? new DefaultDateTimeProvider();
 EnsureDirectory();
 _currentDate = _dateTimeProvider.Now.Date;
 OpenNewWriter(_dateTimeProvider.Now);
 }

 private void EnsureDirectory()
 {
 if (!Directory.Exists(_directory))
 Directory.CreateDirectory(_directory);
 }

 private void OpenNewWriter(DateTime now)
 {
 try
 {
 var path = Path.Combine(_directory, "Log_" + now.ToString("yyyyMMdd_HHmmss_fff") + ".log");
 _writer = File.AppendText(path);
 _writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);
 _writer.AutoFlush = true;
 _currentDate = now.Date;
 }
 catch (Exception ex)
 {
 try { Console.Error.WriteLine($"FileLogWriter: failed to create log file: {ex}"); } catch { }
 _writer = null;
 }
 }

 public void Write(string text)
 {
 try { _writer?.Write(text); } catch { }
 }

 public void WriteLine(string text)
 {
 try { _writer?.WriteLine(text); } catch { }
 }

 public void RotateIfNeeded(DateTime now)
 {
 try
 {
 if (now.Date != _currentDate)
 {
 _writer?.Dispose();
 OpenNewWriter(now);
 }
 }
 catch (Exception ex)
 {
 try { Console.Error.WriteLine($"FileLogWriter: rotate failed: {ex}"); } catch { }
 _writer = null;
 }
 }

 public void Flush()
 {
 try { _writer?.Flush(); } catch { }
 }

 public void Dispose()
 {
 try { _writer?.Flush(); } catch { }
 try { _writer?.Close(); } catch { }
 try { _writer?.Dispose(); } catch { }
 _writer = null;
 }
 }
}
