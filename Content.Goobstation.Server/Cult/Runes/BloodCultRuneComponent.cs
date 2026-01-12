using Content.Goobstation.Shared.Cult.Actions;

namespace Content.Goobstation.Server.Cult.Runes;

[RegisterComponent]
public sealed partial class BloodCultRuneComponent : Component
{
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
