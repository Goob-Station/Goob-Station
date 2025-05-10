using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Components;

/// <summary>
/// Used to mark wounds, that afflict pain; It's calculated automatically from severity and the multiplier
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PainInflicterComponent : Component
{
    /// <summary>
    /// Pain this one exact wound inflicts
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 RawPain;

    /// <summary>
    /// Current pain.
    /// </summary>
    public FixedPoint2 Pain => RawPain * PainMultiplier;

    /// <summary>
    /// What type of pain should this PainInflicter inflict?
    /// </summary>
    [DataField, AutoNetworkedField]
    public PainDamageTypes PainType = PainDamageTypes.WoundPain;

    // Some wounds hurt harder.
    [DataField("multiplier"), ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 PainMultiplier = 1;
}
