using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RustGraspComponent : Component
{
    [DataField]
    public float MinUseDelay = 0.5f;

    [DataField]
    public float MaxUseDelay = 2f;

    [DataField]
    public string Delay = "rust";

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}
