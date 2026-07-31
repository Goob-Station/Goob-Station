// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared._Trauma.Ranching.Components;
using Content.Goobstation.Shared._Trauma.TimedReplace;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared._Trauma.Ranching.Systems;

public sealed partial class EggIncubatorSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    private EntityQuery<TimedReplaceComponent> _eggQuery;

    public override void Initialize()
    {
        base.Initialize();

        _eggQuery = GetEntityQuery<TimedReplaceComponent>();

        SubscribeLocalEvent<EggIncubatorComponent, EntInsertedIntoContainerMessage>(OnPlaced);
        SubscribeLocalEvent<EggIncubatorComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnPlaced(Entity<EggIncubatorComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!_eggQuery.TryComp(args.Entity, out var egg))
            return;

        egg.SpawnTime = _timing.CurTime + egg.Time;
        egg.Active = true;

        _appearance.SetData(ent.Owner, EggIncubatorVisuals.Egg, true);
    }

    private void OnRemoved(Entity<EggIncubatorComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!_eggQuery.TryComp(args.Entity, out var egg))
            return;

        egg.Active = false;
        _appearance.SetData(ent.Owner, EggIncubatorVisuals.Egg, false);
    }
}
