// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Tabletop.Events
{
    /// <summary>
    /// Event to tell other clients that we are dragging this item. Necessery to handle multiple users
    /// trying to move a single item at the same time.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class TabletopDraggingPlayerChangedEvent : EntityEventArgs
    {
        /// <summary>
        /// The UID of the entity being dragged.
        /// </summary>
        public NetEntity DraggedEntityUid;

        public bool IsDragging;

        public TabletopDraggingPlayerChangedEvent(NetEntity draggedEntityUid, bool isDragging)
        {
            DraggedEntityUid = draggedEntityUid;
            IsDragging = isDragging;
        }
    }
}