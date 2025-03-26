using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.SpellCards;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpellCardsActionComponent : Component
{
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
    /// This determines spell use delay, not action component
    /// </summary>
    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(6f);

    /// <summary>
    /// Whether the next spell card burst will be purple or red
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool PurpleCard = false;
}
