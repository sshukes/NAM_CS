using System;
using System.Timers;

using NAudio.Wave;

using NAMStudio.Models;

namespace NAMStudio.Services;

public class AudioEngine : IDisposable
{
    private readonly Timer _levelTimer;
    private readonly object _lock = new();
    private IWavePlayer? _outputDevice;
    private BufferedWaveProvider? _bufferedWaveProvider;
    private NamModel? _model;
    private ImpulseResponse? _impulseResponse;
    private double _inputLevel;
    private double _outputLevel;

    public AudioEngine()
    {
        _levelTimer = new Timer(100);
        _levelTimer.Elapsed += (_, _) => RaiseLevels();
    }

    public event EventHandler<LevelsEventArgs>? LevelsUpdated;

    public double InputGain { get; set; }
    public double OutputGain { get; set; }
    public double WetDryMix { get; set; } = 1;
    public bool NoiseGateEnabled { get; set; }
    public double NoiseGateThresholdDb { get; set; } = -60;
    public bool LimiterEnabled { get; set; } = true;
    public double Tone { get; set; } = 5;
    public double Drive { get; set; } = 5;
    public double Presence { get; set; } = 5;
    public double MasterVolume { get; set; } = 5;
    public bool IsRunning { get; private set; }
    public bool IsBypassed { get; set; }

    public void Configure(AudioSettings settings)
    {
        InputGain = settings.InputGainDb;
        OutputGain = settings.OutputGainDb;
        WetDryMix = settings.WetDryMix;
        NoiseGateEnabled = settings.NoiseGateEnabled;
        NoiseGateThresholdDb = settings.NoiseGateThresholdDb;
        LimiterEnabled = settings.LimiterEnabled;
        Tone = settings.Tone;
        Drive = settings.Drive;
        Presence = settings.Presence;
        MasterVolume = settings.MasterVolume;
    }

    public void SetModel(NamModel model) => _model = model;

    public void SetImpulseResponse(ImpulseResponse impulse) => _impulseResponse = impulse;

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        lock (_lock)
        {
            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 1))
            {
                BufferDuration = TimeSpan.FromSeconds(5)
            };
            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_bufferedWaveProvider);
            _outputDevice.Play();
            _levelTimer.Start();
            IsRunning = true;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            _outputDevice?.Stop();
            _outputDevice?.Dispose();
            _outputDevice = null;
            _bufferedWaveProvider = null;
            _levelTimer.Stop();
            IsRunning = false;
        }
    }

    public void FeedSamples(float[] inputBuffer)
    {
        if (_bufferedWaveProvider is null)
        {
            return;
        }

        var processed = Process(inputBuffer);
        var bytes = new byte[processed.Length * sizeof(float)];
        Buffer.BlockCopy(processed, 0, bytes, 0, bytes.Length);
        _bufferedWaveProvider.AddSamples(bytes, 0, bytes.Length);
    }

    public float[] Process(ReadOnlySpan<float> input)
    {
        var output = new float[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            var sample = input[i];
            _inputLevel = Math.Max(_inputLevel * 0.9, DbFromSample(sample));

            if (NoiseGateEnabled && sample < DbToSample(NoiseGateThresholdDb))
            {
                sample = 0;
            }

            sample *= (float)Math.Pow(10, InputGain / 20);

            if (!IsBypassed)
            {
                sample = ApplyModel(sample);
                sample = ApplyImpulse(sample);
                sample = MixWetDry(sample, input[i]);
            }

            sample *= (float)Math.Pow(10, OutputGain / 20);
            if (LimiterEnabled)
            {
                sample = Math.Clamp(sample, -1.0f, 1.0f);
            }

            output[i] = sample;
            _outputLevel = Math.Max(_outputLevel * 0.9, DbFromSample(sample));
        }

        return output;
    }

    private float ApplyModel(float sample)
    {
        var gain = 1 + (float)(Drive / 10 * 4);
        var presence = 1 + (float)(Presence / 20);
        var tone = (float)Math.Clamp(Tone / 10, 0.2, 1.0);
        var shaped = sample * gain;
        shaped = (float)Math.Tanh(shaped * presence);
        return shaped * tone * (float)(MasterVolume / 10);
    }

    private float ApplyImpulse(float sample)
    {
        if (_impulseResponse is null || _impulseResponse.Taps.Count == 0)
        {
            return sample;
        }

        var acc = 0f;
        foreach (var tap in _impulseResponse.Taps)
        {
            acc += sample * tap;
        }

        return acc;
    }

    private float MixWetDry(float wet, float dry)
    {
        return (float)(wet * WetDryMix + dry * (1 - WetDryMix));
    }

    private static double DbFromSample(float sample)
    {
        var magnitude = Math.Abs(sample);
        return magnitude <= 1e-9 ? -120 : 20 * Math.Log10(magnitude);
    }

    private static float DbToSample(double db) => (float)Math.Pow(10, db / 20);

    private void RaiseLevels()
    {
        LevelsUpdated?.Invoke(this, new LevelsEventArgs(_inputLevel, _outputLevel));
    }

    public void Dispose()
    {
        _levelTimer.Dispose();
        Stop();
    }
}

public class AudioSettings
{
    public double InputGainDb { get; init; }
    public double OutputGainDb { get; init; }
    public double WetDryMix { get; init; }
    public bool NoiseGateEnabled { get; init; }
    public double NoiseGateThresholdDb { get; init; }
    public bool LimiterEnabled { get; init; }
    public double Tone { get; init; }
    public double Drive { get; init; }
    public double Presence { get; init; }
    public double MasterVolume { get; init; }
}

public class LevelsEventArgs : EventArgs
{
    public LevelsEventArgs(double inputLevel, double outputLevel)
    {
        InputLevel = inputLevel;
        OutputLevel = outputLevel;
    }

    public double InputLevel { get; }
    public double OutputLevel { get; }
}
