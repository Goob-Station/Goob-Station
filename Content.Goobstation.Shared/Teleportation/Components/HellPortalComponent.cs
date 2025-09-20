using Content.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Teleportation.Components;

/// <summary>
/// Component for Hell portal activation
/// </summary>
[RegisterComponent]
public sealed partial class HellPortalComponent : Component
{
    public EntityUid? HellMap;
    public EntityUid? ExitPortal;
    public bool PortalEnabled = false;

    [DataField] public string ExitPortalPrototype = "PortalHellExit";
}
