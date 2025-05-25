// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.Lightning;
using Content.Shared._Goobstation.Wizard.TeslaBlast;
using Content.Shared.Electrocution;
using Content.Shared.Physics;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class TeslaBlastSystem : SharedTeslaBlastSystem
{
    [Dependency] private readonly LightningSystem _lightning = default!;

    public override void ShootRandomLightnings(EntityUid performer,
        float power,
        float range,
        int boltCount,
        int arcDepth,
        string lightningPrototype,
        Vector2 minMaxDamage,
        Vector2 minMaxStunTime)
    {
        base.ShootRandomLightnings(performer,
            power,
            range,
            boltCount,
            arcDepth,
            lightningPrototype,
            minMaxDamage,
            minMaxStunTime);

        var damage = float.Lerp(minMaxDamage.X, minMaxDamage.Y, power);
        var stunTime = float.Lerp(minMaxStunTime.X, minMaxStunTime.Y, power);

        var action = new Action<EntityUid>(uid =>
        {
            var preventCollide = EnsureComp<PreventCollideComponent>(uid);
            preventCollide.Uid = performer;

            var electrified = EnsureComp<ElectrifiedComponent>(uid);
            electrified.IgnoredEntity = performer;
            electrified.IgnoreInsulation = true;
            electrified.ShockDamage = damage;
            electrified.ShockTime = stunTime;

            Entity<PreventCollideComponent, ElectrifiedComponent> ent = (uid, preventCollide, electrified);
            Dirty(ent);
        });

        _lightning.ShootRandomLightnings(performer,
            range,
            boltCount,
            lightningPrototype,
            arcDepth,
            false,
            performer,
            action);
    }

    public override void ShootLightning(EntityUid performer,
        EntityUid target,
        string lightningPrototype,
        float damage)
    {
        base.ShootLightning(performer, target, lightningPrototype, damage);

        var action = new Action<EntityUid>(uid =>
        {
            var preventCollide = EnsureComp<PreventCollideComponent>(uid);
            preventCollide.Uid = performer;

            var electrified = EnsureComp<ElectrifiedComponent>(uid);
            electrified.IgnoredEntity = performer;
            electrified.IgnoreInsulation = true;
            electrified.ShockDamage = damage * 2f; // Multiplying by 2 because siemens is 0.5
            electrified.SiemensCoefficient = 0.5f;

            Entity<PreventCollideComponent, ElectrifiedComponent> ent = (uid, preventCollide, electrified);
            Dirty(ent);
        });

        _lightning.ShootLightning(performer, target, lightningPrototype, false, action);
    }
}