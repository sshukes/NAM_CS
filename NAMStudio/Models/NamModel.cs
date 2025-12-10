namespace NAMStudio.Models;

using System;

public class NamModel
{
    public required string Name { get; init; }
    public float[] Weights { get; init; } = Array.Empty<float>();
    public int SampleRate { get; init; } = 48000;
}
