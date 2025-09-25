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
    [DataField]
    public EntityUid? HellMap;

    [ViewVariables]
    public EntityUid ExitPortal;

    [ViewVariables]
    public ResPath HellMapPath = new ResPath("/Maps/_Goobstation/Nonstations/Hell.yml");

    [DataField]
    public bool PortalEnabled;

    [DataField]
    public EntProtoId ExitPortalPrototype = "PortalHellExit";

}
