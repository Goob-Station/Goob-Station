using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.PowerCell;

/// <summary>
///     This component enables power-cell related interactions (e.g., entity white-lists, cell sizes, examine, rigging).
///     The actual power functionality is provided by the server-side BatteryComponent.
/// </summary>
[NetworkedComponent]
[RegisterComponent]
public sealed partial class PowerCellComponent : Component
{
    // Goob edit start
    [DataField]
    public int PowerCellVisualsLevels = 2;
    // Goob edit end
}

[Serializable, NetSerializable]
public enum PowerCellVisuals : byte
{
    ChargeLevel
}
[Serializable, NetSerializable]
public enum PowerCellSlotVisuals : byte
{
    Enabled
}
