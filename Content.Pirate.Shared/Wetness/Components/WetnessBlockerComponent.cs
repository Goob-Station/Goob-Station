using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Worn gear that stops water from reaching the clothing slots it covers.
/// Mirrors <see cref="Content.Pirate.Shared.Stains.Components.StainBlockerComponent"/> with an
/// extra sealed-state gate: hardsuits block whenever worn, modsuits only while sealed.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WetnessBlockerComponent : Component
{
    [DataField("slots", required: true)]
    public SlotFlags BlockedSlots;

    /// <summary>
    /// When true the blocker only applies while its suit is sealed
    /// (<see cref="Content.Goobstation.Shared.Clothing.Components.SealableClothingComponent"/>).
    /// Hardsuits use false; modsuits use true.
    /// </summary>
    [DataField]
    public bool RequiresSealed;
}
