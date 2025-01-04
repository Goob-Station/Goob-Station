using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.SpellCards;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpellCardsActionComponent : Component
{
    [DataField]
    public float LockOnRadius = 3f;

    /// <summary>
    /// How many times the spell can be casted without cooldown resetting
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int UsesLeft = 5;

    /// <summary>
    /// Max uses for this spell before it's cooldown is reset
    /// </summary>
    [DataField]
    public int CastAmount = 5;

    /// <summary>
    /// Whether the next spell card burst will be purple or red
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool PurpleCard = false;
}
