// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Salvage.Components;
using Content.Shared.DragDrop;
using Content.Shared.Storage;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.OreBag;

public sealed class OreBagSystem : EntitySystem
{
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<OreBagComponent, AfterInteractEvent>(OnAfterInteract);

        SubscribeLocalEvent<OreBagComponent, CanDropDraggedEvent>(OnDragAttempt);
        SubscribeLocalEvent<OreBagComponent, DragDropDraggedEvent>(OnDrag);
        SubscribeLocalEvent<OreBagComponent, CanDragEvent>(OnCanDragBox);
    }

    private void OnAfterInteract(EntityUid uid, OreBagComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach
          || args.Target == null
          || !HasComp<MiningPointsLatheComponent>(args.Target)
          || !_timing.IsFirstTimePredicted)
            return;

        if (!TryComp<StorageComponent>(uid, out var storage))
            return;

        var validEntities = new List<EntityUid>();

        foreach (var entity in storage.Container.ContainedEntities)
            if (HasComp<MaterialComponent>(entity))
                validEntities.Add(entity);

        foreach (var entity in validEntities)
            _materialStorage.TryInsertMaterialEntity(args.User, entity, args.Target.Value);
    }

    private void OnDrag(Entity<OreBagComponent> ent, ref DragDropDraggedEvent args)
    {
        if (!TryComp<StorageComponent>(ent.Owner, out var storage) || args.Handled)
            return;

        var validEntities = new List<EntityUid>();

        foreach (var entity in storage.Container.ContainedEntities)
            if (HasComp<MaterialComponent>(entity))
                validEntities.Add(entity);

        foreach (var entity in validEntities)
            _materialStorage.TryInsertMaterialEntity(args.User, entity, args.Target);

        args.Handled = true;
    }

    private void OnDragAttempt(Entity<OreBagComponent> ent, ref CanDropDraggedEvent args)
    {
        if (HasComp<MiningPointsLatheComponent>(args.Target))
            args.CanDrop = true;
        args.Handled = true;
    }

    private void OnCanDragBox(Entity<OreBagComponent> ent, ref CanDragEvent args)
    {
        args.Handled = true;
    }
}
