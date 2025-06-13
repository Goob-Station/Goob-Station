using System;

namespace Content.Pirate.Server.SpecialForces;

public sealed class SpecialForcesHistory
{
    public TimeSpan RoundTime { get; set; }
    public SpecialForcesType Event { get; set; }
    public string WhoCalled { get; set; } = default!;
}
