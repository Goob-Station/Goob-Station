// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs.Systems;
using Content.Trauma.Shared.AnimalAgeing;
using Content.Trauma.Shared.AnimalAgeing.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.Ranching;

/// <summary>
/// This handles raising the age up event on mobs
/// </summary>
public sealed partial class AnimalAgeingSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    private List<Entity<AnimalAgeingComponent>> toAgeUp = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<AnimalAgeingComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<AnimalAgeingComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextAgeTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.AgeTimeMin, ent.Comp.AgeTimeMax));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        toAgeUp.Clear();

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

        foreach (var (uid, ageing) in toAgeUp)
        {
            AttemptAddAgeToMob((uid, ageing));
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
