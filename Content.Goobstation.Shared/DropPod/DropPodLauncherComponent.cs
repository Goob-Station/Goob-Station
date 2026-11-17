using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.DropPod;

[RegisterComponent, Access(typeof(DropPodSystem))]
public sealed partial class DropPodLauncherComponent : Component
{
    [DataField]
    public List<string> Containers = new() { "storagebase", "drop_pod_passengers" };

    [DataField]
    public EntProtoId Pod = DropPodSystem.DefaultPod;
}
