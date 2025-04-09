using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Abilities.Mime;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Mimery;

public sealed class FingerGunSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<FingerGunComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FingerGunComponent, Goobstation.Shared.Mimery.FingerGunEvent>(OnUse);
        SubscribeLocalEvent<FingerGunComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnInit(Entity<FingerGunComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent, ref ent.Comp.FingerGunPower, "ActionFingerGun", ent);
    }

    private void OnComponentShutdown(Entity<FingerGunComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent, ent.Comp.FingerGunPower);
    }


    private void OnUse(Entity<FingerGunComponent> ent, ref Goobstation.Shared.Mimery.FingerGunEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ent.Comp.OnHand = !ent.Comp.OnHand;

        if (!ent.Comp.OnHand)
        {
            if (!ent.Comp.FingerGunExists)
            {
                ent.Comp.FingerGun = Spawn("FingerGun", Transform(ent).Coordinates);
                ent.Comp.FingerGunExists = true;

            }
            if (TryComp<BatteryComponent>(ent.Comp.FingerGun, out var battery))
            {

                ent.Comp.CurrentCharge = ent.Comp.LastCharge + (ent.Comp.ChargeRate * (float)(_gameTiming.CurTime - ent.Comp.LastChargeTime ).TotalSeconds); //Set the charge so that it doesnt spawn with full ammo
                _battery.SetCharge(ent.Comp.FingerGun, ent.Comp.CurrentCharge, battery);
            }
        }
        else
        {

            if (TryComp<BatteryComponent>(ent.Comp.FingerGun, out var battery))
                ent.Comp.LastCharge = battery.CurrentCharge;
            ent.Comp.LastChargeTime = _gameTiming.CurTime;
            Del(ent.Comp.FingerGun);
            ent.Comp.FingerGunExists = false;
        }



        if (!_hands.TryForcePickupAnyHand(ent, ent.Comp.FingerGun))
        {
            Del(ent.Comp.FingerGun);
            return;
        }

    }

}
