// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.TimedReplace;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed class EggIncubatorSystem : EntitySystem
{
    [Dependency] readonly IRobustRandom _random = default!;
    [Dependency] readonly IGameTiming _timing = default!;
    [Dependency] readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EggIncubatorComponent, EntInsertedIntoContainerMessage>(OnPlaced);
        SubscribeLocalEvent<EggIncubatorComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnPlaced(Entity<EggIncubatorComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<TimedReplaceComponent>(args.Entity, out var timedReplace))
            return;

        timedReplace.SpawnTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(timedReplace.MinTime, timedReplace.MaxTime));
        timedReplace.Active = true;

        _appearance.SetData(ent, EggIncubatorVisuals.Egg, true);
    }

    private void OnRemoved(Entity<EggIncubatorComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<TimedReplaceComponent>(args.Entity, out var timedReplace))
            return;

        timedReplace.Active = false;
        _appearance.SetData(ent, EggIncubatorVisuals.Egg, false);
    }
}
