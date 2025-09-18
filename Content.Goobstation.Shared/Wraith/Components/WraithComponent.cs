using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WraithComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Abilities;

    #region Spook Handling
    [ViewVariables]
    public List<EntityUid?> ActiveSpooks;
    #endregion
}
