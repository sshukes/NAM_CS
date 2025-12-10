using System;
using System.IO;
using System.Threading.Tasks;
using NAMStudio.Models;

namespace NAMStudio.Services;

public record NamTrainingRequest(
    string InputWaveformPath,
    string TargetWaveformPath,
    TrainingMetadata Metadata,
    TrainingParameters Parameters
);

public record NamTrainingResult(
    bool Success,
    string NamFilePath,
    double FinalLoss,
    double ValidationLoss,
    double SignalToNoiseRatio,
    string StatusMessage
);

public interface INamTrainer
{
    Task<NamTrainingResult> TrainAsync(NamTrainingRequest request);
}

/// <summary>
/// Placeholder NAM trainer that simulates integrating the official NAM training library.
/// Replace this implementation with a wrapper over the real training API or CLI when available.
/// </summary>
public class StubNamTrainer : INamTrainer
{
    public async Task<NamTrainingResult> TrainAsync(NamTrainingRequest request)
    {
        // Simulate a multi-stage training process.
        await Task.Delay(400);
        var snr = 58 + Random.Shared.NextDouble() * 6;
        await Task.Delay(400);
        var valLoss = Math.Round(0.02 + Random.Shared.NextDouble() * 0.04, 4);
        await Task.Delay(400);
        var finalLoss = Math.Round(0.01 + Random.Shared.NextDouble() * 0.03, 4);

        var outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NAMStudio", "Models");
        Directory.CreateDirectory(outputDirectory);
        var safeName = string.IsNullOrWhiteSpace(request.Metadata.ModelName)
            ? "model"
            : SanitizeFileName(request.Metadata.ModelName);
        var outputFile = Path.Combine(outputDirectory, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.nam");

        // Write a small manifest so users can inspect what was trained.
        var manifest = $"Model: {request.Metadata.ModelName}\n" +
                       $"Amp: {request.Metadata.Amp}\n" +
                       $"Cabinet: {request.Metadata.Cabinet}\n" +
                       $"Microphone: {request.Metadata.Microphone}\n" +
                       $"Session: {request.Metadata.SessionId}\n" +
                       $"Artist: {request.Metadata.Artist}\n" +
                       $"Genre: {request.Metadata.Genre}\n" +
                       $"Notes: {request.Metadata.Notes}\n" +
                       $"Input: {request.InputWaveformPath}\n" +
                       $"Target: {request.TargetWaveformPath}\n" +
                       $"Epochs: {request.Parameters.Epochs}\n" +
                       $"Batch: {request.Parameters.BatchSize}\n" +
                       $"LR: {request.Parameters.LearningRate}\n" +
                       $"TrainSplit: {request.Parameters.TrainSplit}\n" +
                       $"ValSplit: {request.Parameters.ValidationSplit}\n" +
                       $"SampleRate: {request.Parameters.SampleRate}\n" +
                       $"BlockSize: {request.Parameters.BlockSize}\n" +
                       $"L2: {request.Parameters.L2Regularization}\n" +
                       $"Optimizer: {request.Parameters.Optimizer}\n" +
                       $"Normalize: {request.Parameters.NormalizeInput}\n" +
                       $"Augment: {request.Parameters.UseAugmentation}\n" +
                       $"EarlyStopping: {request.Parameters.EarlyStopping} / {request.Parameters.EarlyStoppingPatience}\n";

        await File.WriteAllTextAsync(outputFile, manifest);

        return new NamTrainingResult(
            Success: true,
            NamFilePath: outputFile,
            FinalLoss: finalLoss,
            ValidationLoss: valLoss,
            SignalToNoiseRatio: snr,
            StatusMessage: "Training completed via stubbed NAM trainer"
        );
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
