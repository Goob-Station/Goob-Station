using Content.Shared.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.Clothing;

namespace Content.Pirate.Server.DeviceNetwork;

public sealed class ActivateCameraOnEquippedSystem : EntitySystem
{
    [Dependency] DeviceNetworkSystem _deviceNetworkSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ActivateCameraOnEquippedComponent, ClothingGotEquippedEvent>(onEquipped);
        SubscribeLocalEvent<ActivateCameraOnEquippedComponent, ClothingGotUnequippedEvent>(onUnequipped);
    }
    public void onEquipped(EntityUid uid, ActivateCameraOnEquippedComponent component, ClothingGotEquippedEvent args)
    {
        _deviceNetworkSystem.ConnectDevice(uid);
    }
    public void onUnequipped(EntityUid uid, ActivateCameraOnEquippedComponent component, ClothingGotUnequippedEvent args)
    {
        if (!TryComp(uid, out DeviceNetworkComponent? network_comp))
            return;
        _deviceNetworkSystem.DisconnectDevice(uid, network_comp);
    }
}
