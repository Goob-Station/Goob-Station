using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wires.Components;

/// <summary>
/// This is used for items slots that require entity to have wire panel for interactions
/// </summary>

[RegisterComponent]
[NetworkedComponent]
public sealed partial class ItemSlotsRequirePanelComponent : Component
{
    /// <summary>
    /// For each slot: true - slot require opened panel for interaction, false - slot require closed panel for interaction
    /// </summary>
    [DataField]
    public Dictionary<string, bool> Slots = new();
}
