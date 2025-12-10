using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NAMStudio.Models;

public class TrainingParameters : INotifyPropertyChanged
{
    private int _epochs = 5;
    private int _batchSize = 8;
    private double _learningRate = 0.0005;
    private double _trainSplit = 0.9;
    private double _validationSplit = 0.1;
    private int _sampleRate = 48000;
    private int _blockSize = 2048;
    private double _l2Regularization = 0.0001;
    private bool _earlyStopping = true;
    private int _earlyStoppingPatience = 4;
    private bool _normalizeInput = true;
    private bool _useAugmentation = true;
    private string _optimizer = "Adam";

    public int Epochs
    {
        get => _epochs;
        set => SetField(ref _epochs, value);
    }

    public int BatchSize
    {
        get => _batchSize;
        set => SetField(ref _batchSize, value);
    }

    public double LearningRate
    {
        get => _learningRate;
        set => SetField(ref _learningRate, value);
    }

    public double TrainSplit
    {
        get => _trainSplit;
        set => SetField(ref _trainSplit, value);
    }

    public bool UseAugmentation
    {
        get => _useAugmentation;
        set => SetField(ref _useAugmentation, value);
    }

    public double ValidationSplit
    {
        get => _validationSplit;
        set => SetField(ref _validationSplit, value);
    }

    public int SampleRate
    {
        get => _sampleRate;
        set => SetField(ref _sampleRate, value);
    }

    public int BlockSize
    {
        get => _blockSize;
        set => SetField(ref _blockSize, value);
    }

    public double L2Regularization
    {
        get => _l2Regularization;
        set => SetField(ref _l2Regularization, value);
    }

    public bool EarlyStopping
    {
        get => _earlyStopping;
        set => SetField(ref _earlyStopping, value);
    }

    public int EarlyStoppingPatience
    {
        get => _earlyStoppingPatience;
        set => SetField(ref _earlyStoppingPatience, value);
    }

    public bool NormalizeInput
    {
        get => _normalizeInput;
        set => SetField(ref _normalizeInput, value);
    }

    public string Optimizer
    {
        get => _optimizer;
        set => SetField(ref _optimizer, value);
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
