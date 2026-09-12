using Content.Goobstation.Shared.Hamon.Components;
using Content.Shared.Coordinates;
using Content.Shared.Strip.Components;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Hamon;

/// <summary>
/// Adds the actions to a hamon user
/// </summary>
public partial class HamonSystem
{
    private void InitializeActions()
    {
        SubscribeLocalEvent<HamonUserComponent, MapInitEvent>(OnHamonUserMapInit);
        SubscribeLocalEvent<HamonUserComponent, OverdriveActivateEvent>(OnOverdrive);
        SubscribeLocalEvent<HamonUserComponent, MeleeHitEvent>(OnMeleeHitHamonUser);
        SubscribeLocalEvent<HamonUserComponent, AssPullEvent>(OnAssPull);
    }

    private void OnHamonUserMapInit(Entity<HamonUserComponent> ent, ref MapInitEvent args)
    {
        EntityManager.AddComponents(ent.Owner, ent.Comp.Components);

        foreach (var action in ent.Comp.Actions)
        {
            _actions.AddAction(ent.Owner, action);
        }
    }

    /// <summary>
    /// Comin through comin through comin through now
    /// </summary>
    private void OnOverdrive(Entity<HamonUserComponent> ent, ref OverdriveActivateEvent args)
    {
        args.Handled = true;

        _audio.PlayPredicted(ent.Comp.OverdriveSound, ent.Owner, ent.Owner);
        ent.Comp.OverdriveActive = true;
        EnsureComp<MeleeThrowOnHitComponent>(ent.Owner, out var thrower);
        EnsureComp<HamonInfusedComponent>(ent.Owner);

        thrower.Distance = 15;
        thrower.Speed = 10;
    }

    private void OnMeleeHitHamonUser(Entity<HamonUserComponent> ent, ref MeleeHitEvent args)
    {
        if (!ent.Comp.OverdriveActive)
            return;

        args.BonusDamage += ent.Comp.OverdriveBonusDamage;
        ent.Comp.OverdriveActive = false;
        RemComp<MeleeThrowOnHitComponent>(ent.Owner);

        if (TryComp<HamonInfusedComponent>(ent.Owner, out var infused))
            infused.HamonWearOffTime = _timing.CurTime;
    }

    private void OnAssPull(Entity<HamonUserComponent> ent, ref AssPullEvent args)
    {
        var item = PredictedSpawnAtPosition(ent.Comp.AssPullEntity, ent.Owner.ToCoordinates());
        if (!_hands.TryForcePickupAnyHand(ent.Owner, item))
        {
            _popup.PopupEntity(Loc.GetString("ass-pull-fail"), ent.Owner, ent.Owner);
            QueueDel(item);
            return;
        }

        args.Handled = true;
    }
}
