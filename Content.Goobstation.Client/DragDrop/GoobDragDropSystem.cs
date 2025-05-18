// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Construction;
using Content.Goobstation.Client.Construction;
using Content.Goobstation.Shared.DragDrop;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.DragDrop;

public sealed partial class GoobDragDropSystem : SharedGoobDragDropSystem
{
    [Dependency] private readonly ConstructionSystem _construction = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructionComponent, DragDropTargetEvent>(OnDragDropConstruction);
        SubscribeLocalEvent<ConstructionComponent, CanDropTargetEvent>(CanDropTargetConstruction);

        SubscribeLocalEvent<ConstructionGhostComponent, DragDropTargetEvent>(OnDragDropGhost);
        SubscribeLocalEvent<ConstructionGhostComponent, CanDropTargetEvent>(CanDropTargetGhost);
    }

    // this is cursed but making construction system code handle DragDropTargetEvent would be even more cursed
    // if it works it works
    private void OnDragDropConstruction(Entity<ConstructionComponent> ent, ref DragDropTargetEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        _interaction.InteractUsing(args.User, args.Dragged, ent, Transform(ent).Coordinates);
    }

    private void CanDropTargetConstruction(Entity<ConstructionComponent> ent, ref CanDropTargetEvent args)
    {
        if (HasComp<ItemComponent>(args.Dragged))
        {
            args.CanDrop = true;
            args.Handled = true;
        }
    }

    private void OnDragDropGhost(Entity<ConstructionGhostComponent> ent, ref DragDropTargetEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        _construction.TryStartConstruction(ent, ent.Comp, args.Dragged);
    }

    private void CanDropTargetGhost(Entity<ConstructionGhostComponent> ent, ref CanDropTargetEvent args)
    {
        if (HasComp<ItemComponent>(args.Dragged))
        {
            args.CanDrop = true;
            args.Handled = true;
        }
    }
}
