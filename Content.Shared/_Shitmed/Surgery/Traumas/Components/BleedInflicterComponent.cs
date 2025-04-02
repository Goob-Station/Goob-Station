using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Surgery.Traumas.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BleedInflicterComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public bool IsBleeding = false;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 BleedingAmount => BleedingAmountRaw * Scaling;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 BleedingAmountRaw = 0;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public Dictionary<string, (int Priority, bool CanBleed)> BleedingModifiers = new();
    
    [DataField("canGrow"), ViewVariables(VVAccess.ReadOnly)]
    public bool BleedingScales = true;

    // it's calculated when a wound is spawned.
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan ScalingFinishesAt = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan ScalingStartsAt = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 SeverityPenalty = 1f;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 Scaling = 1;

    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 ScalingLimit = 1.4;
}
