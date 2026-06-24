// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.InternalResources.Events;
using Content.Server.NPC.HTN;
using Content.Shared.Tag;
using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.Ranching.Systems;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.Ranching;

public sealed partial class HappinessSystem : EntitySystem
{
    [Dependency] private HTNSystem _htn = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedHappinessSystem _happiness = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HostileWhenUnhappyComponent, InternalResourcesAmountChangedEvent>(OnHappinessChangedHostile);

        SubscribeLocalEvent<UnhappyWhenCrowdedComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<UnhappyWhenCrowdedComponent>();
        while (query.MoveNext(out var uid, out var replace))
        {
            if (_timing.CurTime < replace.NextUpdate)
                continue;

            var inRange = _lookup.GetEntitiesInRange(uid, replace.Range);

            var count = 0;

            foreach (var entity in inRange)
            {
                if (!_tag.HasTag(entity, replace.Tag))
                    continue;

                count++;
            }

            replace.NextUpdate = _timing.CurTime + replace.UpdateFrequency;

            if (count < replace.MinEntities)
                return;

            if (!TryComp<HappinessComponent>(uid, out var happiness))
                return;

            _happiness.ChangeHappiness((uid, happiness), replace.HappinessToDecrease);
        }
    }

    private void OnMapInit(Entity<UnhappyWhenCrowdedComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateFrequency;
    }

    private void OnHappinessChangedHostile(Entity<HostileWhenUnhappyComponent> ent, ref InternalResourcesAmountChangedEvent args)
    {
        if (!TryComp<HTNComponent>(ent.Owner, out var htn)
            || htn.RootTask == ent.Comp.UnhappyTask && args.NewAmount < ent.Comp.HappinessRequired
            || htn.RootTask == ent.Comp.HappyTask && args.NewAmount > ent.Comp.HappinessRequired)
            return;

        if (args.NewAmount < ent.Comp.HappinessRequired)
            htn.RootTask = ent.Comp.UnhappyTask;

        if (args.NewAmount > ent.Comp.HappinessRequired)
            htn.RootTask = ent.Comp.HappyTask;

        _htn.Replan(htn);
    }
}
