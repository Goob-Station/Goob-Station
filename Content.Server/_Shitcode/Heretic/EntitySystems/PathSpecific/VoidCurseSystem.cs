using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared.Atmos;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;

namespace Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;

public sealed partial class VoidCurseSystem : SharedVoidCurseSystem
{
    [Dependency] private readonly TemperatureSystem _temp = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<VoidCurseComponent>();
        var deletionqueue = new List<EntityUid>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (comp.Lifetime <= 0)
                deletionqueue.Add(uid);

            comp.Timer -= frameTime;
            if (comp.Timer > 0)
                continue;

            comp.Timer = 1f;
            comp.Lifetime -= 1f;

            Cycle((uid, comp));
        }

        foreach (var q in deletionqueue)
            RemComp<VoidCurseComponent>(q);
    }

    protected override void Cycle(Entity<VoidCurseComponent> ent)
    {
        if (TryComp<TemperatureComponent>(ent, out var temp))
        {
            // temperaturesystem is not idiotproof :(
            var t = temp.CurrentTemperature - (2f * ent.Comp.Stacks);
            _temp.ForceChangeTemperature(ent, Math.Clamp(t, Atmospherics.TCMB, Atmospherics.Tmax), temp);
        }

        _statusEffect.TryAddStatusEffect<MutedComponent>(ent, "Muted", TimeSpan.FromSeconds(5), true);
    }
}
