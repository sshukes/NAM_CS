using System.Collections.Generic;

namespace NAMStudio.Models;

public class ImpulseResponse
{
    public required string Name { get; init; }
public List<float> Taps { get; init; } = new();
}
