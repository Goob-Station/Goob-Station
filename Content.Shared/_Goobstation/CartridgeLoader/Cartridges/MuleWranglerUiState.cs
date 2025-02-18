using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class MuleWranglerUiState(
    List<NetEntity> mules,
    List<NetEntity> beacons,
    NetEntity selectedMule,
    NetEntity selectedBeacon) : BoundUserInterfaceState
{
    public List<NetEntity> Mules = mules;
    public List<NetEntity> Beacons = beacons;
    public NetEntity SelectedMule = selectedMule;
    public NetEntity SelectedBeacon = selectedBeacon;
}
