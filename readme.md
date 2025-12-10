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

Use the UI to load `.nam` models and impulse responses (`.wav`/`.irs`), tweak tone controls, manage presets, and start or stop audio playback. Presets are stored in `%APPDATA%/NAMStudio/Presets` as JSON. The **Navigate** menu in the title area lets you jump between the Live Rig and Training workflows without scrolling.

### Training clean/noisy waveforms into a NAM file

Open the **Training** tab to prepare a dataset and export a new `.nam` snapshot. Provide a clean waveform and a noisy waveform, enter NAM metadata (model name, artist, genre, notes), adjust training parameters (epochs, batch size, learning rate, train split, augmentation), and click **Train and Export NAM**. Each run is listed in the Training Runs grid with status, metrics, and the generated NAM file path. Selecting a run and choosing **Open** or **View Selected** shows a detail window with the metadata, parameters, metrics, and export location.

## Troubleshooting

- If you see `System.IO.IOException` related to audio devices, ensure your input and output devices are available and not locked by other applications.
- Avoid using the `Spacing` attribute on `StackPanel` or other panels when targeting older WPF versions; the project removes these to stay broadly compatible. Use `Margin` to control layout spacing if you copy or extend the UI.
