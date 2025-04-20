using Content.Shared.FixedPoint;
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
    public FixedPoint2 Pain;

    // Some wounds hurt harder.
    [DataField("multiplier"), ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 PainMultiplier = 1;
}
