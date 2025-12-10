namespace NAMStudio.Models;

public class TrainingParameters
{
    public int Epochs { get; set; } = 5;

    public int BatchSize { get; set; } = 8;

    public double LearningRate { get; set; } = 0.0005;

    public double TrainSplit { get; set; } = 0.9;

    public bool UseAugmentation { get; set; } = true;

    public double ValidationSplit { get; set; } = 0.1;

    public int SampleRate { get; set; } = 48000;

    public int BlockSize { get; set; } = 2048;

    public double L2Regularization { get; set; } = 0.0001;

    public bool EarlyStopping { get; set; } = true;

    public int EarlyStoppingPatience { get; set; } = 4;

    public bool NormalizeInput { get; set; } = true;

    public string Optimizer { get; set; } = "Adam";
}
