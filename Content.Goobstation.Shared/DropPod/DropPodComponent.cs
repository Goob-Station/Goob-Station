using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.DropPod;

[RegisterComponent, AutoGenerateComponentPause, Access(typeof(DropPodSystem))]
public sealed partial class DropPodComponent : Component
{
    public const string ContainerId = "drop_pod_contents";

    [DataField]
    public List<EntProtoId> Contents = new();

    [DataField]
    public EntityTableSelector? Table;

    [DataField]
    public TimeSpan LandDelay = TimeSpan.FromSeconds(9.75);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LandTime;
}
