namespace NAMStudio.Models;

public class TrainingParameters
{
    public int Epochs { get; set; } = 5;

    public int BatchSize { get; set; } = 8;

    public double LearningRate { get; set; } = 0.0005;

    public double TrainSplit { get; set; } = 0.9;

    public bool UseAugmentation { get; set; } = true;
}
