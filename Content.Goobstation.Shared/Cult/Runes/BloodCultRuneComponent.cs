using Content.Goobstation.Shared.Cult.Events;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Cult.Runes;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultRuneComponent : Component
{
    /// <summary>
    ///     What will the rune do when activated.
    /// </summary>
    [DataField(required: true)] public List<CultRuneEvent> Events;
}
