

using Content.Server.Mind;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;
using Content.Shared._Shitcode.Mimery;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Server._Shitcode.Mimery;

public sealed class FingerGunSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<FingerGunComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FingerGunComponent, FingerGunEvent>(OnUse);
    }

    private void OnInit(Entity<FingerGunComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent, "ActionFingerGun");
    }

    private void OnUse(Entity<FingerGunComponent> ent, ref FingerGunEvent args)
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
