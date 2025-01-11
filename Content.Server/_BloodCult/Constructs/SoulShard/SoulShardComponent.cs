using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Constructs.SoulShard;

[RegisterComponent]
public sealed partial class SoulShardComponent : Component
{
    [DataField]
    public bool IsBlessed;

    [DataField]
    public Color BlessedLightColor = Color.LightCyan;

    [DataField]
    public EntProtoId ShadeProto = "ShadeCult";

    [DataField]
    public EntProtoId PurifiedShadeProto = "ShadeHoly";

    public EntityUid? ShadeUid;
}
