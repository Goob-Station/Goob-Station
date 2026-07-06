using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Worn gear that blocks water from covered slots.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WetnessBlockerComponent : Component
{
    [DataField("slots", required: true)]
    public SlotFlags BlockedSlots;

    /// <summary>
    /// When true, only sealed suits block water.
    /// </summary>
    [DataField]
    public bool RequiresSealed;
}
