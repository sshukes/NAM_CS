using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

using NAMStudio.Models;

namespace NAMStudio.Services;

public class ModelService
{
    public string BrowseForModel()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "NAM Models (*.nam)|*.nam|All files (*.*)|*.*",
            Title = "Select Neural Amp Model"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
    }

    public async Task<NamModel> LoadModelAsync(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Model file not found", path);
        }

        var bytes = await File.ReadAllBytesAsync(path);
        var weights = Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes)).Select(b => (float)b / byte.MaxValue).ToArray();
        return new NamModel
        {
            Name = Path.GetFileNameWithoutExtension(path),
            Weights = weights,
            SampleRate = 48000
        };
    }

    public void LoadModelIfExists(string path, AudioEngine engine)
    {
        if (File.Exists(path))
        {
            Task.Run(async () =>
            {
                var model = await LoadModelAsync(path);
                Application.Current.Dispatcher.Invoke(() => engine.SetModel(model));
            });
        }
    }
}
