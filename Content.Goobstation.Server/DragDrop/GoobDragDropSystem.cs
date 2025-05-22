// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.DragDrop;
using Content.Server.Construction.Components;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.DragDrop;

public sealed partial class GoobDragDropSystem : SharedGoobDragDropSystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructionComponent, DragDropTargetEvent>(OnDragDropConstruction);
    }

    // this is cursed but making construction system code handle DragDropTargetEvent would be even more cursed
    // if it works it works
    private void OnDragDropConstruction(Entity<ConstructionComponent> ent, ref DragDropTargetEvent args)
    {
        _interaction.InteractUsing(args.User, args.Dragged, ent, Transform(ent).Coordinates);
    }
}
