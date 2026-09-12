using Content.Goobstation.Shared.Hamon.Components;
using Content.Shared.Slippery;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Hamon;

public partial class HamonSystem
{
    private void InitializeInfused()
    {
        SubscribeLocalEvent<HamonUserComponent, InfuseWithHamonEvent>(OnInfuseWithHamon);
        SubscribeLocalEvent<HamonInfusedComponent, MapInitEvent>(OnInfusedMapInit);

        // Effects
        SubscribeLocalEvent<AddComponentsOnHamonInfusedComponent, GotInfusedWithHamonEvent>(OnReflectInfused);
        SubscribeLocalEvent<HamonUserComponent, SlipAttemptEvent>(OnSlip);
        SubscribeLocalEvent<HamonInfusedComponent, MeleeHitEvent>(OnMeleeHitInfused);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HamonInfusedComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.HamonWearOffTime == TimeSpan.Zero || comp.HamonWearOffTime > _timing.CurTime)
                continue;

            comp.HamonWearOffTime = TimeSpan.Zero;
            var ev = new GotUnInfusedWithHamonEvent();
            RaiseLocalEvent(uid, ref ev);

            RemComp<HamonInfusedComponent>(uid);

            if (TryComp<AddComponentsOnHamonInfusedComponent>(uid, out var hamonComps))
                EntityManager.RemoveComponents(uid, hamonComps.Components);
        }
    }

    private void OnInfuseWithHamon(Entity<HamonUserComponent> ent, ref InfuseWithHamonEvent args)
    {
        if (!TryComp<HamonInfuseableComponent>(args.Target, out var infuse) || HasComp<HamonInfusedComponent>(args.Target))
            return;

        args.Handled = true;

        EnsureComp<HamonInfusedComponent>(args.Target, out var hamon);

        hamon.HamonWearOffTime = _timing.CurTime + infuse.InfuseTime * ent.Comp.InfuseBonusTime;

        var ev = new GotInfusedWithHamonEvent();
        RaiseLocalEvent(args.Target, ref ev);
    }

    private void OnSlip(Entity<HamonUserComponent> ent, ref SlipAttemptEvent args)
    {
        args.NoSlip = true;
    }

    private void OnInfusedMapInit(Entity<HamonInfusedComponent> ent, ref MapInitEvent args)
    {
        _audio.PlayPredicted(ent.Comp.Sound, ent.Owner, ent.Owner);
    }

    private void OnReflectInfused(Entity<AddComponentsOnHamonInfusedComponent> ent, ref GotInfusedWithHamonEvent args)
    {
        EntityManager.AddComponents(ent.Owner, ent.Comp.Components);
    }

    private void OnMeleeHitInfused(Entity<HamonInfusedComponent> ent, ref MeleeHitEvent args)
    {
        args.BonusDamage += ent.Comp.BonusDamage;
    }
}
