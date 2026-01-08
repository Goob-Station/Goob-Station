using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.WraithPoints;

[RegisterComponent, NetworkedComponent, Access(typeof(WraithPointsSystem))]
[AutoGenerateComponentState]
public sealed partial class WraithPointsComponent : Component
{
    /// <summary>
    ///  Current wraith points the entity has
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 WraithPoints;

    /// <summary>
    /// How many wraith points the entity starts with
    /// </summary>
    [DataField(required: true)]
    public FixedPoint2 StartingWraithPoints;

    [DataField]
    public ProtoId<AlertPrototype> Alert = "WraithPoints";
}
