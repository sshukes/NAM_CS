using System;
using System.IO;
using System.Threading.Tasks;

using NAMStudio.Models;

namespace NAMStudio.Services;

public class TrainingService
{
    public async Task TrainAsync(TrainingRun run)
    {
        run.Status = "Running";
        run.CreatedAt = DateTimeOffset.Now;

        // Simulate work with staged metric updates.
        await Task.Delay(500);
        run.SignalToNoiseRatio = 60 + Random.Shared.NextDouble() * 10;
        await Task.Delay(500);
        run.ValidationLoss = Math.Round(0.02 + Random.Shared.NextDouble() * 0.05, 4);
        await Task.Delay(500);
        run.FinalLoss = Math.Round(0.01 + Random.Shared.NextDouble() * 0.03, 4);

        var outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NAMStudio", "Models");
        Directory.CreateDirectory(outputDirectory);
        var safeName = string.IsNullOrWhiteSpace(run.Metadata.ModelName) ? "model" : SanitizeFileName(run.Metadata.ModelName);
        var outputFile = Path.Combine(outputDirectory, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.nam");

        await File.WriteAllTextAsync(outputFile, "Stub NAM model content");

        run.NamFilePath = outputFile;
        run.Status = "Completed";
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name;
    }
}
