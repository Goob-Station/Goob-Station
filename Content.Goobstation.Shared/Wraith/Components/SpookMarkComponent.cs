using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpookMarkComponent : Component
{
    [DataField]
    public EntProtoId Spook = "SpookObject";

    [DataField]
    public EntityUid? SpookEntity;
}
