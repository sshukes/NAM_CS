namespace NAMStudio.Models;

public class Preset
{
    public string Name { get; set; } = "Untitled";
    public string ModelPath { get; set; } = string.Empty;
    public string IrPath { get; set; } = string.Empty;
    public double InputGain { get; set; }
    public double OutputGain { get; set; }
    public double WetDryMix { get; set; }
    public double Tone { get; set; }
    public double Drive { get; set; }
    public double Presence { get; set; }
    public double MasterVolume { get; set; }
    public double NoiseGateThreshold { get; set; }
    public bool NoiseGateEnabled { get; set; }
    public bool LimiterEnabled { get; set; }
}
