// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Tabletop.Events
{
    /// <summary>
    /// An event ot tell the server that we have stopped playing this tabletop game.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class TabletopStopPlayingEvent : EntityEventArgs
    {
        /// <summary>
        /// The entity UID of the table associated with this tabletop game.
        /// </summary>
        public NetEntity TableUid;

        public TabletopStopPlayingEvent(NetEntity tableUid)
        {
            TableUid = tableUid;
        }
    }
}