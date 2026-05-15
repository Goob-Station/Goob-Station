// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Medical.Common.Vomiting;
using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.Ranching.Events;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed partial class VomitCounterSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<VomitCounterComponent, VomitedEvent>(OnVomited);
        SubscribeLocalEvent<VomitCounterComponent, RanchingEggLayEvent>(OnEgglayed);
    }

    private void OnEgglayed(Entity<VomitCounterComponent> ent, ref RanchingEggLayEvent args)
    {
        ent.Comp.TimesVomited = 0;

        RemComp<VomitedEnoughMarkerComponent>(ent.Owner);
    }

    private void OnVomited(Entity<VomitCounterComponent> ent, ref VomitedEvent args)
    {
        ent.Comp.TimesVomited++;

        if (ent.Comp.TimesVomited >= ent.Comp.NeededVomits)
            EnsureComp<VomitedEnoughMarkerComponent>(ent.Owner);
    }
}
