using Content.Goobstation.Shared.Cult.Events;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Cult.Runes;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultRuneComponent : Component
{
    [DataField] public float InvokersLookupRange = 1.5f;

    [DataField] public float TargetsLookupRange = 1f;

    /// <summary>
    ///     What will the rune do when activated.
    /// </summary>
    [DataField(required: true)] public List<CultRuneEvent> Events;
}
