using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Medical.Healing;

namespace Content.Shared.Medical._Goobstation;

/// <summary>
/// Event raised when a wound is being medicated.
/// </summary>
[ByRefEvent]
public record struct HealingSystemEvent(EntityUid Healing, EntityUid Entity)
{
    public bool AnythingToDo {get; set; } = false;
}
