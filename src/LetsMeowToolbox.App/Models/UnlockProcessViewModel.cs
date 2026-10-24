namespace LetsMeowToolbox.App.Models;

/// <summary>占用进程展示模型。</summary>
public sealed class UnlockProcessViewModel : ObservableObject
{
    public int ProcessId { get; init; }
    public string ProcessName { get; init; } = string.Empty;
    public string AppName { get; init; } = string.Empty;
    public bool IsService { get; init; }
    public bool Restartable { get; init; }

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set { Set(ref _status, value); OnPropertyChanged(nameof(HasStatus)); }
    }
    public bool HasStatus => !string.IsNullOrEmpty(_status);
}
