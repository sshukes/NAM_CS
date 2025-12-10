using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using NAMStudio.Models;
using NAMStudio.Services;

namespace NAMStudio.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ModelService _modelService = new();
    private readonly ImpulseResponseService _impulseResponseService = new();
    private readonly AudioEngine _audioEngine = new();
    private readonly PresetService _presetService = new();
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

        _audioEngine.LevelsUpdated += OnLevelsUpdated;
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

    public ObservableCollection<Preset> Presets { get; } = new();

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
