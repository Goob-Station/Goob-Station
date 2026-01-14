using Content.Goobstation.Shared.Cult.Events;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Cult.Runes;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultRuneComponent : Component
{
    /// <summary>
    ///     What color the rune will be.
    /// </summary>
    [DataField] public Color Color = Color.MediumPurple;

    /// <summary>
    ///     How much people do you need around the rune for it to work.
    /// </summary>
    [DataField(required: true)] public int RequiredInvokers = 1;

    /// <summary>
    ///     What will the rune do when activated.
    /// </summary>
    [DataField(required: true)] public CultRuneEvent Event;

    /// <summary>
    ///     What invokers will say on success.
    /// </summary>
    [DataField] public string InvokeLoc = string.Empty;
}
