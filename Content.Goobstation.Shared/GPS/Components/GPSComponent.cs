using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.GPS.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GPSComponent : Component
{
    [DataField, AutoNetworkedField]
    public string GpsName { get; set; } = "";

    [DataField, AutoNetworkedField]
    public bool InDistress { get; set; } = false;

    [DataField, AutoNetworkedField]
    public NetEntity? TrackedEntity { get; set; }
}
