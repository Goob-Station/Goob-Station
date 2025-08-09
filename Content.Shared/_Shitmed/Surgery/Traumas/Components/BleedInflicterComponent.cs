using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BleedInflicterComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public bool IsBleeding = false;

    /// <summary>
    ///     The severity it requires for the wound to have, so bleeds can be induced
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 SeverityThreshold = FixedPoint2.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 BleedingAmount => BleedingAmountRaw * Scaling;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 BleedingAmountRaw = FixedPoint2.Zero;

    // these are calculated when wound is spawned.
    /// <summary>
    ///     The time at which the scaling of bleeding started
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan ScalingFinishesAt = TimeSpan.Zero;

    /// <summary>
    ///     The time at which the scaling will end
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan ScalingStartsAt = TimeSpan.Zero;

    [DataField]
    public FixedPoint2 ScalingSpeed = FixedPoint2.New(1);

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 SeverityPenalty = FixedPoint2.New(1);

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 Scaling = FixedPoint2.New(1);

    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 ScalingLimit = FixedPoint2.New(1.4);

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public Dictionary<string, (int Priority, bool CanBleed)> BleedingModifiers = new();
}
