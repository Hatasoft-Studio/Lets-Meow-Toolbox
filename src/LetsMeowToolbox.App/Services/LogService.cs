using System.IO;
using System.Text;

namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 全局操作日志系统（单例）。记录用户的所有设置操作、功能执行结果与运行错误。
/// 日志文件：%LocalAppData%\LetsMeowToolbox\logs\meow-yyyyMMdd.log
/// 级别：INFO / WARN / ERROR，均带时间戳（yyyy-MM-dd HH:mm:ss）。
/// </summary>
public sealed class LogService : IDisposable
{
    private static readonly Lazy<LogService> _instance = new(() => new LogService());
    /// <summary>全局日志入口。应用启动后即可使用。</summary>
    public static LogService Instance => _instance.Value;

    private readonly object _lock = new();
    private readonly string _dir;
    private readonly string _file;
    private StreamWriter? _writer;

    private LogService()
    {
        _dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LetsMeowToolbox", "logs");
        Directory.CreateDirectory(_dir);
        _file = Path.Combine(_dir, $"meow-{DateTime.Now:yyyyMMdd}.log");
        try
        {
            _writer = new StreamWriter(_file, append: true, Encoding.UTF8) { AutoFlush = true };
        }
        catch
        {
            _writer = null;
        }
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        lock (_lock)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            try { _writer?.WriteLine(line); } catch { /* 日志写入失败不影响主流程 */ }
        }
    }

    /// <summary>日志文件所在目录。</summary>
    public string LogDirectory => _dir;

    /// <summary>当前日志文件完整路径。</summary>
    public string LogFilePath => _file;

    public void Dispose()
    {
        lock (_lock)
        {
            try { _writer?.Dispose(); } catch { /* ignore */ }
            _writer = null;
        }
    }
}
