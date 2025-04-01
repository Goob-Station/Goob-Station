using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RustGraspComponent : Component
{
    [DataField]
    public float MinUseDelay = 0.7f;

    [DataField]
    public float MaxUseDelay = 3f;

    [DataField]
    public float CatwalkDelayMultiplier = 0.15f;

    [DataField]
    public string Delay = "rust";

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}
