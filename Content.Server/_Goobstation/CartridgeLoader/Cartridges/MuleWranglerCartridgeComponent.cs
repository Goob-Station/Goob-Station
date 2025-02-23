namespace Content.Server._Goobstation.CartridgeLoader.Cartridges;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
[Access(typeof(MuleWranglerCartridgeSystem))]
public sealed partial class MuleWranglerCartridgeComponent : Component
{
    public NetEntity SelectedMule = NetEntity.Invalid;

    public NetEntity SelectedBeacon = NetEntity.Invalid;
}
