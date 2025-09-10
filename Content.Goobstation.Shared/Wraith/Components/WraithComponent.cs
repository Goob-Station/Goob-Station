using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WraithComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Abilities;

    #region Blood Writing
    [ViewVariables(VVAccess.ReadWrite), DataField("bloodWritingCost")]
    public FixedPoint2 BloodWritingCost = -2;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public EntityUid? BloodCrayon;

    #endregion
}
