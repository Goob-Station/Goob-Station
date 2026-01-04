using Content.Shared.EntityEffects;

namespace Content.Goobstation.Server.Cult;

[RegisterComponent]
public sealed partial class BloodCultRuneComponent : Component
{
    /// <summary>
    ///     How much people do you need around the rune for it to work.
    /// </summary>
    [DataField] public int RequiredInvokers = 1;

    /// <summary>
    ///     What will the rune do when activated.
    /// </summary>
    [DataField] public List<EntityEventArgs> Events = new();
}
