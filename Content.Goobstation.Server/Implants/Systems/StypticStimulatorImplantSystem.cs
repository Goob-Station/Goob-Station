using System.Linq;
using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Damage;
using Content.Shared.Implants;
using Content.Shared.Mobs.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class StypticStimulatorImplantSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }

    private void OnImplant(Entity<StypticStimulatorImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        ent.Comp.NextDamage = _timing.CurTime + TimeSpan.FromSeconds(1f);
    }

    // Every tick, attempt to damage entities
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        // Go through every entity with the component
        var query = EntityQueryEnumerator<StypticStimulatorImplantComponent, DamageableComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var comp, out var damage, out var mobState))
        {
            // Make sure they're up for a damage tick
            if (comp.NextDamage > curTime)
                continue;

            if (comp.DamageCap != 0 && damage.TotalDamage >= comp.DamageCap)
                continue;

            // Set the next time they can take damage
            comp.NextDamage = curTime + TimeSpan.FromSeconds(1f);

            // Damage them
            foreach (var allowedState in comp.AllowedStates.Where(allowedState => allowedState == mobState.CurrentState)) // IM LINQING IT!!
            {
                _damageable.TryChangeDamage(uid, comp.Damage, true, false, damage);
            }
        }
    }

    private void OnUnimplanted(Entity<StypticStimulatorImplantComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (HasComp<StypticStimulatorImplantComponent>(args.Implant))
            RemComp<StypticStimulatorImplantComponent>(ent);
    }
}
