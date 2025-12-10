using System.Threading.Tasks;

using NAMStudio.Models;

namespace NAMStudio.Services;

public class TrainingService
{
    private readonly INamTrainer _trainer;

    public TrainingService(INamTrainer? trainer = null)
    {
        _trainer = trainer ?? new StubNamTrainer();
    }

    public async Task TrainAsync(TrainingRun run)
    {
        run.Status = "Running";
        run.CreatedAt = DateTimeOffset.Now;
        run.StatusMessage = "Submitting training request to NAM trainer";

        var request = new NamTrainingRequest(
            run.InputWaveformPath,
            run.TargetWaveformPath,
            run.Metadata,
            run.Parameters);

        var result = await _trainer.TrainAsync(request);

        run.SignalToNoiseRatio = result.SignalToNoiseRatio;
        run.ValidationLoss = result.ValidationLoss;
        run.FinalLoss = result.FinalLoss;
        run.NamFilePath = result.NamFilePath;
        run.Status = result.Success ? "Completed" : "Failed";
        run.StatusMessage = result.StatusMessage;
    }
}
