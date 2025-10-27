// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Marker;
// Lavaland Change
using Content.Server._Lavaland.Pressure;
using Content.Shared._Lavaland.Weapons.Marker;
using Content.Shared._White.BackStab;
using Content.Shared.Damage;
using Content.Shared.Stunnable;

namespace Content.Server.Weapons;

public sealed class DamageMarkerSystem : SharedDamageMarkerSystem
{
    // Lavaland Change Start
    [Dependency] private readonly PressureEfficiencyChangeSystem _pressure = default!;
    [Dependency] private readonly BackStabSystem _backstab = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageMarkerComponent, ApplyMarkerBonusEvent>(OnApplyMarkerBonus);
    }

    private void OnApplyMarkerBonus(EntityUid uid, DamageMarkerComponent component, ref ApplyMarkerBonusEvent args)
    {
        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return;

        if (TryComp<DamageBoostOnMarkerComponent>(args.Weapon, out var boost))
        {
            var pressureMultiplier = 1f;

            if (TryComp<PressureDamageChangeComponent>(args.Weapon, out var pressure)
                && _pressure.ApplyModifier((args.Weapon, pressure)))
                pressureMultiplier = pressure.AppliedModifier;

            if (boost.BackstabBoost != null
                && _backstab.TryBackstab(uid, args.User, Angle.FromDegrees(45d), playSound: false))
                _damageable.TryChangeDamage(uid,
                (boost.BackstabBoost + boost.Boost) * pressureMultiplier,
                damageable: damageable,
                origin: args.User);
            else
                _damageable.TryChangeDamage(uid,
                boost.Boost * pressureMultiplier,
                damageable: damageable,
                origin: args.User);
        }
    }
    // Lavaland Change End
}