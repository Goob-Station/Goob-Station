
using Content.Shared._F14.SCPOS;
using Robust.Shared.GameStates;

namespace Content.Server._F14.SCPOS;

[RegisterComponent]
public sealed partial class SCPOSComputerComponent : Component
{
    [DataField]
    public int DiskCapacity = 64;

    [DataField]
    public int DiskUsed = 0;

    [DataField]
    public HashSet<string> InstalledSoftware = new();
}
