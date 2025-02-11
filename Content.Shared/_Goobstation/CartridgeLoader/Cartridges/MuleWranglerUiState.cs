using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class MuleWranglerUiState : BoundUserInterfaceState
{
    public List<NetEntity> Mules = new();

    public MuleWranglerUiState(List<NetEntity> mules)
    {
        Mules = mules;
    }
}
