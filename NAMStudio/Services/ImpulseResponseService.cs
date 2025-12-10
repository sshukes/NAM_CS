using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using NAudio.Wave;

using NAMStudio.Models;

namespace NAMStudio.Services;

public class ImpulseResponseService
{
    public string BrowseForImpulseResponse()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Impulse Responses (*.wav;*.irs)|*.wav;*.irs|All files (*.*)|*.*",
            Title = "Select Impulse Response"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
    }

    public async Task<ImpulseResponse> LoadImpulseResponseAsync(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Impulse response not found", path);
        }

        var taps = new List<float>();
        await using var reader = new AudioFileReader(path);
        var buffer = new float[reader.WaveFormat.SampleRate];
        var read = await reader.ReadAsync(buffer, 0, buffer.Length);
        for (var i = 0; i < read; i++)
        {
            taps.Add(buffer[i]);
        }

        return new ImpulseResponse
        {
            Name = Path.GetFileNameWithoutExtension(path),
            Taps = taps
        };
    }

    public void LoadImpulseIfExists(string path, AudioEngine engine)
    {
        if (File.Exists(path))
        {
            Task.Run(async () =>
            {
                var impulse = await LoadImpulseResponseAsync(path);
                Application.Current.Dispatcher.Invoke(() => engine.SetImpulseResponse(impulse));
            });
        }
    }
}
