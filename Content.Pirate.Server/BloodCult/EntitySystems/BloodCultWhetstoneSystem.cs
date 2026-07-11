// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.BloodCult.Components;
using Content.Shared.BloodCult;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Weapons.Melee;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class BloodCultWhetstoneSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultWhetstoneComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<BloodCultWhetstoneComponent> stone, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { } target || stone.Comp.Uses <= 0 ||
            !TryComp<MeleeWeaponComponent>(target, out var melee) ||
            !HasComp<ItemComponent>(target) ||
            _whitelist.IsValid(stone.Comp.Blacklist, target) ||
            !_whitelist.IsValid(stone.Comp.Whitelist, target))
            return;

        var changed = false;
        foreach (var (damageType, increase) in stone.Comp.DamageIncrease.DamageDict)
        {
            if (!melee.Damage.DamageDict.TryGetValue(damageType, out var current) || current >= stone.Comp.MaximumIncrease)
                continue;

            var updated = current + increase;
            if (updated > stone.Comp.MaximumIncrease)
                updated = stone.Comp.MaximumIncrease;

            melee.Damage.DamageDict[damageType] = updated;
            changed = true;
        }

        if (!changed)
            return;

        Dirty(target, melee);
        _audio.PlayPvs(stone.Comp.SharpenSound, target);

        stone.Comp.Uses--;
        if (stone.Comp.Uses <= 0)
            _appearance.SetData(stone, BloodCultVisuals.Active, false);

        args.Handled = true;
    }
}
