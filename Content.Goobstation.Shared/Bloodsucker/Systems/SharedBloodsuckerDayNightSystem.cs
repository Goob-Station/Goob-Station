using Content.Goobstation.Shared.Bloodsuckers.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Raised when day begins (solar flare starts).
/// </summary>
[ByRefEvent]
public record struct BloodsuckerDayStartedEvent;

/// <summary>
/// Raised when night begins (solar flare ends).
/// </summary>
[ByRefEvent]
public record struct BloodsuckerNightStartedEvent;

/// <summary>
/// Raised at warning thresholds before day starts.
/// </summary>
[ByRefEvent]
public record struct BloodsuckerDayWarningEvent(float SecondsRemaining);

public abstract class SharedBloodsuckerDayNightSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected float RollNightDuration(BloodsuckerDayNightComponent comp)
        => _random.NextFloat(comp.NightDurationMin, comp.NightDurationMax);

    protected float RollDayDuration(BloodsuckerDayNightComponent comp)
        => _random.NextFloat(comp.DayDurationMin, comp.DayDurationMax);
}
