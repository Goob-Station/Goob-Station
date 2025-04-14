using Content.Goobstation.Server.Religion.Nullrod;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.Weapons.ReloadOnPray;

public sealed partial class ReloadOnPraySystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReloadOnPrayComponent, NullrodPrayEvent>(OnPray);
    }

    private void OnPray(EntityUid uid, ReloadOnPrayComponent comp, ref NullrodPrayEvent args)
    {
        if (!TryComp<BasicEntityAmmoProviderComponent>(uid, out var ammoProvider))
            return;

        ammoProvider.Count = ammoProvider.Capacity;
        _audioSystem.PlayPvs(comp.ReloadSoundPath, uid);
    }


}
