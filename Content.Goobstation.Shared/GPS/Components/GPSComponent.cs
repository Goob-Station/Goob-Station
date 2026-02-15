using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.GPS.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true)]
public sealed partial class GPSComponent : Component
{
    [DataField, AutoNetworkedField]
    public string GpsName = "";

    [DataField, AutoNetworkedField]
    public List<GpsEntry> GpsEntries = new();

    [DataField, AutoNetworkedField]
    public NetEntity? TrackedEntity;

    [DataField, AutoNetworkedField]
    public bool InDistress;

    [DataField, AutoNetworkedField]
    public bool Enabled;
}
