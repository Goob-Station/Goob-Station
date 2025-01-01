using System.Numerics;
using Content.Server.Lightning;
using Content.Shared._Goobstation.Wizard.TeslaBlast;
using Content.Shared.Electrocution;
using Content.Shared.Physics;

namespace Content.Server._Goobstation.Wizard;

public sealed class TeslaBlastSystem : SharedTeslaBlastSystem
{
    [Dependency] private readonly LightningSystem _lightning = default!;

    protected override void ShootRandomLightnings(EntityUid performer,
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
            electrified.IgnoredEntity = uid;
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
}
