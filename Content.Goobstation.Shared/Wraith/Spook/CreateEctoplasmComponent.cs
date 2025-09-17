using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Spook;

[RegisterComponent, NetworkedComponent]
public sealed partial class CreateEctoplasmComponent : Component
{
    [DataField]
    public EntProtoId EctoplasmProto = "Ectoplasm";

    /// <summary>
    /// Minimum and Maximum amounts of ectoplasm to spawn
    /// </summary>
    [DataField]
    public Vector2i AmountMinMax = new(3, 6);
}
