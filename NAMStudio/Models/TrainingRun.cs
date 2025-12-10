using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NAMStudio.Models;

public class TrainingRun : INotifyPropertyChanged
{
    private string _status = "Queued";
    private string _namFilePath = string.Empty;
    private double _finalLoss;
    private double _validationLoss;
    private double _signalToNoiseRatio;
    private DateTimeOffset _createdAt = DateTimeOffset.Now;
    private string _statusMessage = string.Empty;
    private string _inputWaveformPath = string.Empty;
    private string _targetWaveformPath = string.Empty;

    public string InputWaveformPath
    {
        get => _inputWaveformPath;
        set => SetField(ref _inputWaveformPath, value);
    }

    public string TargetWaveformPath
    {
        get => _targetWaveformPath;
        set => SetField(ref _targetWaveformPath, value);
    }

    public TrainingMetadata Metadata { get; set; } = new();

    public TrainingParameters Parameters { get; set; } = new();

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string NamFilePath
    {
        get => _namFilePath;
        set => SetField(ref _namFilePath, value);
    }

    public double FinalLoss
    {
        get => _finalLoss;
        set => SetField(ref _finalLoss, value);
    }

    public double ValidationLoss
    {
        get => _validationLoss;
        set => SetField(ref _validationLoss, value);
    }

    public double SignalToNoiseRatio
    {
        get => _signalToNoiseRatio;
        set => SetField(ref _signalToNoiseRatio, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        set => SetField(ref _createdAt, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
