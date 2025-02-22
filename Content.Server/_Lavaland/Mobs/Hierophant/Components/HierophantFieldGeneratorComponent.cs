using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantFieldGeneratorComponent : Component
{
    [ViewVariables]
    public bool Enabled;

    [ViewVariables]
    public List<EntityUid> Walls = new();

    [DataField]
    public int Radius;

    [DataField]
    public EntProtoId HierophantPrototype = "LavalandBossHierophant";

    [DataField]
    public EntProtoId WallPrototype = "WallHierophantArenaTemporary";

    [DataField]
    public EntityUid? ConnectedHierophant;
}
