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

Use the UI to load `.nam` models and impulse responses (`.wav`/`.irs`), tweak tone controls, manage presets, and start or stop audio playback. Presets are stored in `%APPDATA%/NAMStudio/Presets` as JSON.
