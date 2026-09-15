// SPDX-License-Identifier: MIT

using Robust.Shared.Containers;

namespace Content.Shared._Oskarrr.HyperSleep;

/// <summary>
/// Updates open/closed visuals when someone enters or leaves a hypersleep chamber.
/// </summary>
public sealed class HyperSleepChamberSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HyperSleepChamberComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<HyperSleepChamberComponent, EntRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<HyperSleepChamberComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<HyperSleepChamberComponent> ent, ref ComponentStartup args)
    {
        _appearance.SetData(ent, HyperSleepChamberVisuals.Occupied, false);
    }

    private void OnInserted(Entity<HyperSleepChamberComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        _appearance.SetData(ent, HyperSleepChamberVisuals.Occupied, true);
    }

    private void OnRemoved(Entity<HyperSleepChamberComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        _appearance.SetData(ent, HyperSleepChamberVisuals.Occupied, args.Container.ContainedEntities.Count > 0);
    }
}
