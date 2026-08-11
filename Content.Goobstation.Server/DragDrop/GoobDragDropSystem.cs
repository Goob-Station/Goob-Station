// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.DragDrop;
using Content.Server.Construction.Components;
using Content.Shared.Climbing.Systems;
using Content.Shared.DragDrop;

namespace Content.Goobstation.Server.DragDrop;

public sealed class GoobDragDropSystem : SharedGoobDragDropSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructionComponent, DragDropTargetEvent>(OnDragDropConstruction, after: [typeof(ClimbSystem)]);
        SubscribeLocalEvent<DragDropTargetableComponent, DragDropTargetEvent>(OnDragDropTargetable, after: [typeof(ClimbSystem)]);
    }

    // this is cursed but making construction system code handle DragDropTargetEvent would be even more cursed
    // if it works it works
    private void OnDragDropConstruction(Entity<ConstructionComponent> ent, ref DragDropTargetEvent args)
    {
        OnDragDrop(ent, ref args);
    }

    private void OnDragDropTargetable(Entity<DragDropTargetableComponent> ent, ref DragDropTargetEvent args)
    {
        OnDragDrop(ent, ref args);
    }
}
