using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;

using NAMStudio;
using NAMStudio.Models;
using NAMStudio.Services;

namespace NAMStudio.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ModelService _modelService = new();
    private readonly ImpulseResponseService _impulseResponseService = new();
    private readonly AudioEngine _audioEngine = new();
    private readonly PresetService _presetService = new();
    private readonly TrainingService _trainingService = new();
    private string _modelPath = "";
    private string _irPath = "";
    private double _inputGain = 0;
    private double _outputGain = 0;
    private double _wetDryMix = 1;
    private double _tone = 5;
    private double _drive = 5;
    private double _presence = 5;
    private double _masterVolume = 5;
    private double _noiseGateThreshold = -60;
    private bool _noiseGateEnabled;
    private bool _limiterEnabled = true;
    private bool _isBypassed;
    private string _statusMessage = "Ready";
    private double _inputLevel;
    private double _outputLevel;
    private Preset? _selectedPreset;
    private string _cleanWaveformPath = string.Empty;
    private string _noisyWaveformPath = string.Empty;
    private TrainingRun? _selectedTrainingRun;

    public MainViewModel()
    {
        LoadModelCommand = new RelayCommand(async _ => await LoadModelAsync());
        LoadIrCommand = new RelayCommand(async _ => await LoadIrAsync());
        StartAudioCommand = new RelayCommand(_ => StartAudio(), _ => !_audioEngine.IsRunning);
        StopAudioCommand = new RelayCommand(_ => StopAudio(), _ => _audioEngine.IsRunning);
        SavePresetCommand = new RelayCommand(_ => SavePreset());
        LoadPresetCommand = new RelayCommand(_ => LoadPreset());
        DeletePresetCommand = new RelayCommand(_ => DeletePreset(), _ => SelectedPreset is not null);
        RenamePresetCommand = new RelayCommand(_ => RenamePreset(), _ => SelectedPreset is not null);

        BrowseCleanWaveformCommand = new RelayCommand(_ => BrowseCleanWaveform());
        BrowseNoisyWaveformCommand = new RelayCommand(_ => BrowseNoisyWaveform());
        StartTrainingCommand = new RelayCommand(async _ => await StartTrainingAsync(), _ => CanStartTraining());
        OpenRunDetailsCommand = new RelayCommand(OpenRunDetails, parameter => parameter is TrainingRun || SelectedTrainingRun is not null);

        _audioEngine.LevelsUpdated += OnLevelsUpdated;
        TrainingMetadata.PropertyChanged += (_, _) => (StartTrainingCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand LoadModelCommand { get; }
    public ICommand LoadIrCommand { get; }
    public ICommand StartAudioCommand { get; }
    public ICommand StopAudioCommand { get; }
    public ICommand SavePresetCommand { get; }
    public ICommand LoadPresetCommand { get; }
    public ICommand DeletePresetCommand { get; }
    public ICommand RenamePresetCommand { get; }
    public ICommand BrowseCleanWaveformCommand { get; }
    public ICommand BrowseNoisyWaveformCommand { get; }
    public ICommand StartTrainingCommand { get; }
    public ICommand OpenRunDetailsCommand { get; }

    public ObservableCollection<Preset> Presets { get; } = new();
    public ObservableCollection<TrainingRun> TrainingRuns { get; } = new();

    public TrainingMetadata TrainingMetadata { get; } = new();

    public TrainingParameters TrainingParameters { get; } = new();

    public string ModelPath
    {
        get => _modelPath;
        set => SetField(ref _modelPath, value);
    }

    public string IrPath
    {
        get => _irPath;
        set => SetField(ref _irPath, value);
    }

    public double InputGain
    {
        get => _inputGain;
        set
        {
            if (SetField(ref _inputGain, value))
            {
                _audioEngine.InputGain = value;
            }
        }
    }

    public double OutputGain
    {
        get => _outputGain;
        set
        {
            if (SetField(ref _outputGain, value))
            {
                _audioEngine.OutputGain = value;
            }
        }
    }

    public double WetDryMix
    {
        get => _wetDryMix;
        set
        {
            if (SetField(ref _wetDryMix, value))
            {
                _audioEngine.WetDryMix = value;
            }
        }
    }

    public double Tone
    {
        get => _tone;
        set => SetField(ref _tone, value);
    }

    public double Drive
    {
        get => _drive;
        set => SetField(ref _drive, value);
    }

    public double Presence
    {
        get => _presence;
        set => SetField(ref _presence, value);
    }

    public double MasterVolume
    {
        get => _masterVolume;
        set => SetField(ref _masterVolume, value);
    }

    public double NoiseGateThreshold
    {
        get => _noiseGateThreshold;
        set => SetField(ref _noiseGateThreshold, value);
    }

    public bool NoiseGateEnabled
    {
        get => _noiseGateEnabled;
        set => SetField(ref _noiseGateEnabled, value);
    }

    public bool LimiterEnabled
    {
        get => _limiterEnabled;
        set => SetField(ref _limiterEnabled, value);
    }

    public bool IsBypassed
    {
        get => _isBypassed;
        set
        {
            if (SetField(ref _isBypassed, value))
            {
                _audioEngine.IsBypassed = value;
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public double InputLevel
    {
        get => _inputLevel;
        set => SetField(ref _inputLevel, value);
    }

    public double OutputLevel
    {
        get => _outputLevel;
        set => SetField(ref _outputLevel, value);
    }

    public Preset? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (SetField(ref _selectedPreset, value))
            {
                (DeletePresetCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RenamePresetCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string CleanWaveformPath
    {
        get => _cleanWaveformPath;
        set
        {
            if (SetField(ref _cleanWaveformPath, value))
            {
                (StartTrainingCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string NoisyWaveformPath
    {
        get => _noisyWaveformPath;
        set
        {
            if (SetField(ref _noisyWaveformPath, value))
            {
                (StartTrainingCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public TrainingRun? SelectedTrainingRun
    {
        get => _selectedTrainingRun;
        set
        {
            if (SetField(ref _selectedTrainingRun, value))
            {
                (OpenRunDetailsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    private async Task LoadModelAsync()
    {
        var path = _modelService.BrowseForModel();
        if (string.IsNullOrWhiteSpace(path))
        {
            StatusMessage = "Model load cancelled";
            return;
        }

        ModelPath = path;
        var model = await _modelService.LoadModelAsync(path);
        _audioEngine.SetModel(model);
        StatusMessage = $"Loaded model: {model.Name}";
    }

    private async Task LoadIrAsync()
    {
        var path = _impulseResponseService.BrowseForImpulseResponse();
        if (string.IsNullOrWhiteSpace(path))
        {
            StatusMessage = "IR load cancelled";
            return;
        }

        IrPath = path;
        var ir = await _impulseResponseService.LoadImpulseResponseAsync(path);
        _audioEngine.SetImpulseResponse(ir);
        StatusMessage = $"Loaded IR: {ir.Name}";
    }

    private void StartAudio()
    {
        _audioEngine.Configure(new AudioSettings
        {
            InputGainDb = InputGain,
            OutputGainDb = OutputGain,
            WetDryMix = WetDryMix,
            NoiseGateEnabled = NoiseGateEnabled,
            NoiseGateThresholdDb = NoiseGateThreshold,
            LimiterEnabled = LimiterEnabled,
            Tone = Tone,
            Drive = Drive,
            Presence = Presence,
            MasterVolume = MasterVolume
        });

        _audioEngine.Start();
        (StartAudioCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (StopAudioCommand as RelayCommand)?.RaiseCanExecuteChanged();
        StatusMessage = "Audio engine started";
    }

    private void StopAudio()
    {
        _audioEngine.Stop();
        (StartAudioCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (StopAudioCommand as RelayCommand)?.RaiseCanExecuteChanged();
        StatusMessage = "Audio engine stopped";
    }

    private void SavePreset()
    {
        var preset = new Preset
        {
            Name = _presetService.PromptForPresetName(),
            ModelPath = ModelPath,
            IrPath = IrPath,
            InputGain = InputGain,
            OutputGain = OutputGain,
            WetDryMix = WetDryMix,
            Tone = Tone,
            Drive = Drive,
            Presence = Presence,
            MasterVolume = MasterVolume,
            NoiseGateThreshold = NoiseGateThreshold,
            NoiseGateEnabled = NoiseGateEnabled,
            LimiterEnabled = LimiterEnabled
        };

        if (string.IsNullOrWhiteSpace(preset.Name))
        {
            StatusMessage = "Preset not saved";
            return;
        }

        _presetService.SavePreset(preset);
        Presets.Add(preset);
        StatusMessage = $"Saved preset '{preset.Name}'";
    }

    private void LoadPreset()
    {
        var preset = _presetService.LoadPresetFromDisk();
        if (preset is null)
        {
            StatusMessage = "Preset load cancelled";
            return;
        }

        ApplyPreset(preset);
        StatusMessage = $"Loaded preset '{preset.Name}'";
    }

    private void DeletePreset()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        _presetService.DeletePreset(SelectedPreset);
        Presets.Remove(SelectedPreset);
        StatusMessage = "Preset deleted";
    }

    private void RenamePreset()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        var newName = _presetService.PromptForPresetName(SelectedPreset.Name);
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        SelectedPreset.Name = newName;
        _presetService.SavePreset(SelectedPreset);
        OnPropertyChanged(nameof(Presets));
        StatusMessage = "Preset renamed";
    }

    private void ApplyPreset(Preset preset)
    {
        ModelPath = preset.ModelPath;
        IrPath = preset.IrPath;
        InputGain = preset.InputGain;
        OutputGain = preset.OutputGain;
        WetDryMix = preset.WetDryMix;
        Tone = preset.Tone;
        Drive = preset.Drive;
        Presence = preset.Presence;
        MasterVolume = preset.MasterVolume;
        NoiseGateThreshold = preset.NoiseGateThreshold;
        NoiseGateEnabled = preset.NoiseGateEnabled;
        LimiterEnabled = preset.LimiterEnabled;
        _modelService.LoadModelIfExists(ModelPath, _audioEngine);
        _impulseResponseService.LoadImpulseIfExists(IrPath, _audioEngine);
    }

    private void BrowseCleanWaveform()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Waveform|*.wav;*.flac;*.aiff;*.aif|All files|*.*",
            Title = "Select clean waveform"
        };

        if (dialog.ShowDialog() == true)
        {
            CleanWaveformPath = dialog.FileName;
            StatusMessage = "Loaded clean waveform";
        }
    }

    private void BrowseNoisyWaveform()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Waveform|*.wav;*.flac;*.aiff;*.aif|All files|*.*",
            Title = "Select noisy waveform"
        };

        if (dialog.ShowDialog() == true)
        {
            NoisyWaveformPath = dialog.FileName;
            StatusMessage = "Loaded noisy waveform";
        }
    }

    private bool CanStartTraining()
    {
        return !string.IsNullOrWhiteSpace(CleanWaveformPath)
            && !string.IsNullOrWhiteSpace(NoisyWaveformPath)
            && !string.IsNullOrWhiteSpace(TrainingMetadata.ModelName);
    }

    private async Task StartTrainingAsync()
    {
        if (!CanStartTraining())
        {
            StatusMessage = "Provide waveforms and model name before training";
            return;
        }

        var run = new TrainingRun
        {
            CleanWaveformPath = CleanWaveformPath,
            NoisyWaveformPath = NoisyWaveformPath,
            Metadata = CloneMetadata(TrainingMetadata),
            Parameters = CloneParameters(TrainingParameters),
            Status = "Queued",
            CreatedAt = DateTimeOffset.Now
        };

        TrainingRuns.Insert(0, run);
        SelectedTrainingRun = run;
        StatusMessage = "Training started...";

        await _trainingService.TrainAsync(run);

        StatusMessage = string.IsNullOrWhiteSpace(run.NamFilePath)
            ? "Training finished"
            : $"Training finished: {run.NamFilePath}";
    }

    private static TrainingMetadata CloneMetadata(TrainingMetadata metadata)
    {
        return new TrainingMetadata
        {
            Artist = metadata.Artist,
            Genre = metadata.Genre,
            ModelName = metadata.ModelName,
            Notes = metadata.Notes
        };
    }

    private static TrainingParameters CloneParameters(TrainingParameters parameters)
    {
        return new TrainingParameters
        {
            BatchSize = parameters.BatchSize,
            Epochs = parameters.Epochs,
            LearningRate = parameters.LearningRate,
            TrainSplit = parameters.TrainSplit,
            UseAugmentation = parameters.UseAugmentation
        };
    }

    private void OpenRunDetails(object? parameter)
    {
        var run = parameter as TrainingRun ?? SelectedTrainingRun;
        if (run is null)
        {
            StatusMessage = "Select a training run first";
            return;
        }

        var window = new TrainingRunDetailsWindow(run)
        {
            Owner = Application.Current?.MainWindow
        };

        window.ShowDialog();
    }

    private void OnLevelsUpdated(object? sender, LevelsEventArgs e)
    {
        InputLevel = e.InputLevel;
        OutputLevel = e.OutputLevel;
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Func<object?, bool>? _canExecute;
    private readonly Action<object?> _execute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
