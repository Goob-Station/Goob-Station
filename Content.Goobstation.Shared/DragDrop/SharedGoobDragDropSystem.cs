// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DragDrop;
using Content.Shared.Item;

namespace Content.Goobstation.Shared.DragDrop;

public abstract partial class SharedGoobDragDropSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemComponent, CanDragEvent>(CanDragItem);
    }

    // needed for drag-and-drop construction
    private void CanDragItem(Entity<ItemComponent> ent, ref CanDragEvent args)
    {
        args.Handled = true;
    }
}
