// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <43253663+DogZeroX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <le0nel_1van@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2024 778b <33431126+778b@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Weapons.Ranged;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    protected override void InitializeMagazine()
    {
        base.InitializeMagazine();
        SubscribeLocalEvent<MagazineAmmoProviderComponent, UpdateAmmoCounterEvent>(OnMagazineAmmoUpdate);
        SubscribeLocalEvent<MagazineAmmoProviderComponent, AmmoCounterControlEvent>(OnMagazineControl);
    }

    private void OnMagazineAmmoUpdate(EntityUid uid, MagazineAmmoProviderComponent component, UpdateAmmoCounterEvent args)
    {
        var ent = GetMagazineEntity(uid);

        if (ent == null)
        {
            if (args.Control is DefaultStatusControl control)
            {
                control.Update(0, 0);
            }

            return;
        }

        RaiseLocalEvent(ent.Value, args, false);
    }

    private void OnMagazineControl(EntityUid uid, MagazineAmmoProviderComponent component, AmmoCounterControlEvent args)
    {
        var ent = GetMagazineEntity(uid);
        if (ent == null)
            return;
        RaiseLocalEvent(ent.Value, args, false);
    }
}