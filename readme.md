# NAMStudio (C# WPF)

This repository contains a standalone Windows desktop version of the Neural Amp Modeler UI written in C# with WPF. The app mirrors the model/IR loading, tone shaping, preset management, and live audio processing flow from the earlier Python + React prototype.

## Project layout

- `NAMStudio/NAMStudio.csproj` – .NET 8.0 Windows WPF project file.
- `NAMStudio/MainWindow.xaml` – Main UI surface for loading models/IRs, adjusting controls, and starting/stopping the audio engine.
- `NAMStudio/ViewModels/MainViewModel.cs` – MVVM logic, command wiring, state management, and preset interactions.
- `NAMStudio/Services` – Supporting services for audio processing, preset persistence, model loading, and IR handling.
- `NAMStudio/Models` – Simple data representations for models, impulse responses, and presets.

## Building

1. Install the .NET 8.0 SDK and enable Windows desktop workloads.
2. Open the folder in Visual Studio Code (or Visual Studio) on Windows.
3. Restore dependencies and build:
   ```bash
   dotnet restore NAMStudio/NAMStudio.csproj
   dotnet build NAMStudio/NAMStudio.csproj
   ```

## Running

From a Windows terminal:

```bash
dotnet run --project NAMStudio/NAMStudio.csproj
```

### Running and debugging in Visual Studio Code

1. Install the C# Dev Kit and .NET Install Tool extensions when prompted.
2. Open the repo folder in VS Code (`File → Open Folder…`).
3. Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>B</kbd> and choose **build** to restore and compile the project (tasks are defined in `.vscode/tasks.json`).
4. Start debugging with <kbd>F5</kbd> using the **NAMStudio (.NET Launch)** configuration; VS Code will build and launch `NAMStudio.exe` from `bin/Debug/net8.0-windows`.

Use the UI to load `.nam` models and impulse responses (`.wav`/`.irs`), tweak tone controls, manage presets, and start or stop audio playback. Presets are stored in `%APPDATA%/NAMStudio/Presets` as JSON. Use the top **Live Rig**/**Training** menu items or the quick navigation bar beneath them to jump between workflows without scrolling.

### Training input/target waveforms into a NAM file

- Use the top **Training Window** button or menu entry to open the dedicated **Training Workspace**. It provides separate pages for configuring a run and browsing finished runs/exports.
- On the **Run Training** page, load an **Input** waveform (clean) and a **Target** waveform (captured/amp output), fill in NAM metadata (model name, artist, genre, amp, cabinet, microphone, session id, notes), and tune training parameters (epochs, batch size, learning rate, train/validation split, sample rate, block size, optimizer, normalization, augmentation, early stopping/patience).
- Click **Train and Export NAM** (or **Start New Training**) to queue the run. Runs are listed with input/target paths, status, metrics, and the exported NAM file path; double-click **Open Selected Details** or **View Details** to inspect the metadata, parameters, and metrics captured by the trainer.
- The `TrainingService` uses a pluggable `INamTrainer` abstraction. The default `StubNamTrainer` simulates the official NAM trainer and writes a manifest alongside the generated `.nam` file. Replace it with a wrapper over the real NAM training library/CLI when available.

## Troubleshooting

- If you see `System.IO.IOException` related to audio devices, ensure your input and output devices are available and not locked by other applications.
- Avoid using the `Spacing` attribute on `StackPanel` or other panels when targeting older WPF versions; the project removes these to stay broadly compatible. Use `Margin` to control layout spacing if you copy or extend the UI.
