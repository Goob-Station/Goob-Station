// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.OnPray.ReloadOnPray;

public sealed partial class ReloadOnPraySystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReloadOnPrayComponent, NullrodPrayEvent>(OnPray);
    }

    private void OnPray(EntityUid uid, ReloadOnPrayComponent comp, ref NullrodPrayEvent args)
    {
        if (!TryComp<BasicEntityAmmoProviderComponent>(uid, out var ammoProvider) || ammoProvider.Capacity == null)
            return;

        if (_gun.UpdateBasicEntityAmmoCount(uid, ammoProvider.Capacity.Value, ammoProvider))
            _audioSystem.PlayPvs(comp.ReloadSoundPath, uid);
    }
}
