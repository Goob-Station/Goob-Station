using Content.Shared._ES.Camera;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    [Dependency] private ESScreenshakeSystem _shake = default!;

    private void GunScreenshake(EntityUid user, Entity<GunComponent> gun)
    {
        var rotation = new ESScreenshakeParameters(0.085f * gun.Comp.CameraRecoilScalarModified, 1.2f, 0.008f);
        _shake.Screenshake(user, null, rotation);
    }
}
