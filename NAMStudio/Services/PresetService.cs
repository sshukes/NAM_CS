using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.VisualBasic;

using NAMStudio.Models;

namespace NAMStudio.Services;

public class PresetService
{
    private readonly string _presetDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NAMStudio",
        "Presets");

    public PresetService()
    {
        Directory.CreateDirectory(_presetDirectory);
    }

    public string PromptForPresetName(string? initialName = null)
    {
        return Interaction.InputBox("Preset Name", "Save Preset", initialName ?? "New Preset");
    }

    public void SavePreset(Preset preset)
    {
        var path = GetPresetPath(preset.Name);
        var json = JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public Preset? LoadPresetFromDisk()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Preset files (*.json)|*.json|All files (*.*)|*.*",
            InitialDirectory = _presetDirectory
        };

        if (dialog.ShowDialog() != true)
        {
            return null;
        }

        var json = File.ReadAllText(dialog.FileName);
        return JsonSerializer.Deserialize<Preset>(json);
    }

    public void DeletePreset(Preset preset)
    {
        var path = GetPresetPath(preset.Name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetPresetPath(string name)
    {
        var safeName = Regex.Replace(name, "[^a-zA-Z0-9_-]", "_");
        return Path.Combine(_presetDirectory, $"{safeName}.json");
    }
}
