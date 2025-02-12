using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class MuleWranglerUiState(List<NetEntity> mules, List<NetEntity> beacons) : BoundUserInterfaceState
{
    public List<NetEntity> Mules = mules;
    public List<NetEntity> Beacons = beacons;
    /*
    public NetEntity SelectedMule = beacons;
    public NetEntity SelectedBeacon = beacons; saveState
    */
}
