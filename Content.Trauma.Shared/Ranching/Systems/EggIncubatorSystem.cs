// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.TimedReplace;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed partial class EggIncubatorSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private EntityQuery<TimedReplaceComponent> _eggQuery = default!;

    [SubscribeLocalEvent]
    private void OnPlaced(Entity<EggIncubatorComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!_eggQuery.TryComp(args.Entity, out var egg))
            return;

        egg.SpawnTime = _timing.CurTime + egg.Time;
        egg.Active = true;

        _appearance.SetData(ent.Owner, EggIncubatorVisuals.Egg, true);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<EggIncubatorComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!_eggQuery.TryComp(args.Entity, out var egg))
            return;

        egg.Active = false;
        _appearance.SetData(ent.Owner, EggIncubatorVisuals.Egg, false);
    }
}
