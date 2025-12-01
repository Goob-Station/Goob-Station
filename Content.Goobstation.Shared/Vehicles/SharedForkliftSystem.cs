// SPDX-FileCopyrightText...
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DragDrop;
using Content.Shared.Tag;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Vehicles;

public sealed class ForkliftSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private const string CrateContainerId = "crate_storage";
    private const string CrateTag = "Crate";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ForkliftComponent, ComponentInit>(OnUpdate);
        SubscribeLocalEvent<ForkliftComponent, EntInsertedIntoContainerMessage>(OnUpdate);
        SubscribeLocalEvent<ForkliftComponent, EntRemovedFromContainerMessage>(OnUpdate);
        SubscribeLocalEvent<ForkliftComponent, DragDropTargetEvent>(DragDropped);
    }

    private void DragDropped(Entity<ForkliftComponent> ent, ref DragDropTargetEvent args)
    {
        if(args.Handled || !_container.TryGetContainer(ent, CrateContainerId, out var container))
            return;

        var transform = Transform(args.Dragged);
        _container.Insert((args.Dragged, transform), container);
        Log.Debug("TEST WE BALl");
        args.Handled = true;
    }


    private void OnUpdate<T>(Entity<ForkliftComponent> ent, ref T args)
    {
        UpdateAppearance(ent);
    }

    private void UpdateAppearance(EntityUid uid)
    {

        if(!_container.TryGetContainer(uid, CrateContainerId, out var container))
            return;

        var state = container.ContainedEntities.Count switch
        {
            0 => ForkliftCrateState.Empty,
            1 => ForkliftCrateState.OneCrate,
            2 => ForkliftCrateState.TwoCrates,
            3 => ForkliftCrateState.ThreeCrates,
            _ => ForkliftCrateState.FourCrates,
        };

        _appearance.SetData(uid, ForkliftVisuals.CrateState, state);
    }
}
