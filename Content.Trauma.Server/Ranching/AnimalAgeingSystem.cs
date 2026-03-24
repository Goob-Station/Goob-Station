using Content.Shared.Mobs.Systems;
using Content.Trauma.Shared.AnimalAgeing;
using Content.Trauma.Shared.AnimalAgeing.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.Ranching;

/// <summary>
/// This handles raising the age up event on mobs
/// </summary>
public sealed class AnimalAgeingSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toAgeUp = new List<(EntityUid uid, AnimalAgeingComponent comp)>();

        var query = EntityQueryEnumerator<AnimalAgeingComponent>();
        while (query.MoveNext(out var uid, out var ageComp))
        {
            if (_mobState.IsDead(uid) || _mobState.IsCritical(uid))
                continue;

            if (_timing.CurTime < ageComp.NextAgeTime)
                continue;

            ageComp.NextAgeTime += TimeSpan.FromSeconds(_random.NextFloat(ageComp.AgeTimeMin, ageComp.AgeTimeMax));

            toAgeUp.Add((uid, ageComp));
        }

        foreach (var (uid, eggLayer) in toAgeUp)
        {
            AttemptAddAgeToMob((uid, eggLayer));
        }
    }

    public void AttemptAddAgeToMob(Entity<AnimalAgeingComponent> ent)
    {
        var attemptev = new AddAgeToMobAttemptEvent(ent, ent.Comp.YearsPerUpdate);
        RaiseLocalEvent(ent.Owner, ref attemptev);

        if (attemptev.Cancelled)
            return;

        var ev = new AddAgeToMobEvent(ent, attemptev.Years);
        RaiseLocalEvent(ent.Owner, ref ev);
    }
}
