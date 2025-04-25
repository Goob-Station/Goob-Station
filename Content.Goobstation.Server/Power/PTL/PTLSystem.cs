using Content.Server.Flash;
using Content.Server.Power.Components;
using Content.Server.Power.SMES;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Radiation.Components;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Server.Power.PTL;

public sealed partial class PTLSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SmesSystem));
        SubscribeLocalEvent<PTLComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<PTLComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<PTLComponent, ExaminedEvent>(OnExamine);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<PTLComponent>();

        while (eqe.MoveNext(out var uid, out var ptl))
        {
            if (_time.CurTime > ptl.NextShotAt)
            {
                ptl.NextShotAt = _time.CurTime + ptl.ShootDelay;
                Tick((uid, ptl));
            }
        }
    }

    private void Tick(Entity<PTLComponent> ent)
    {
        if (!TryComp<BatteryComponent>(ent, out var battery)
        || battery.CurrentCharge < ent.Comp.MinShootPower)
            return;

        Shoot(ent);
    }

    private void Shoot(Entity<PTLComponent> ent)
    {
        var megajoule = 1000000;

        var charge = 0f;
        var spesos = 0f;

        if (TryComp<BatteryComponent>(ent, out var battery))
        {
            charge = battery.CurrentCharge / megajoule;
            // taken from paradise wiki. i don't know what these numbers are doing either just take it as given
            spesos = (int) (2 * charge / (2 * charge + 50)); // 50 is minimum credits earned
        }
        if (charge < 1f) return;

        // scale damage from energy
        if (TryComp<HitscanBatteryAmmoProviderComponent>(ent, out var hitscan))
        {
            hitscan.FireCost = charge;
            var prot = _protMan.Index<HitscanPrototype>(hitscan.Prototype);
            prot.Damage = ent.Comp.BaseBeamDamage * charge * 2f;
        }

        if (TryComp<GunComponent>(ent, out var gun))
        {
            var forward = new EntityCoordinates(ent, new Vector2(0, -1));
            _gun.AttemptShoot(ent, ent, gun, forward);
        }

        // EVIL behavior......
        if (charge >= ent.Comp.PowerEvilThreshold)
        {
            var evil = charge / ent.Comp.PowerEvilThreshold;

            if (TryComp<RadiationSourceComponent>(ent, out var rad))
                rad.Intensity = evil;

            var lookup = _lookup.GetEntitiesInRange<MobStateComponent>(Transform(ent).Coordinates, evil * 2.5f);
            foreach (var bozo in lookup)
                // flashing through walls? no biggie. only the sound of the laser firing should discombobulate nearby people...
                _flash.FlashArea((ent, null), ent, evil, evil);
        }

        ent.Comp.SpesosHeld += spesos;
    }

    private void OnChargeChanged(Entity<PTLComponent> ent, ref ChargeChangedEvent args)
    {

    }

    private void OnAfterInteractUsing(Entity<PTLComponent> ent, ref AfterInteractUsingEvent args)
    {
        // if holding a wrench rotate it
    }

    private void OnExamine(Entity<PTLComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"It holds [color=yellow]{ent.Comp.SpesosHeld} spesos[/color]. Alt-click to collect!");
    }
}
