// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common._Trauma.Medical.Vomiting;
using Content.Goobstation.Shared._Trauma.Ranching.Components;
using Content.Goobstation.Shared._Trauma.Ranching.Events;

namespace Content.Goobstation.Shared._Trauma.Ranching.Systems;

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
