// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server.Tabletop
{
    /// <summary>
    ///     A class for storing data about a running tabletop game.
    /// </summary>
    public sealed class TabletopSession
    {
        /// <summary>
        ///     The center position of this session.
        /// </summary>
        public readonly MapCoordinates Position;

        /// <summary>
        ///     The set of players currently playing this tabletop game.
        /// </summary>
        public readonly Dictionary<ICommonSession, TabletopSessionPlayerData> Players = new();

        /// <summary>
        ///     All entities bound to this session. If you create an entity for this session, you have to add it here.
        /// </summary>
        public readonly HashSet<EntityUid> Entities = new();

        public TabletopSession(MapId tabletopMap, Vector2 position)
        {
            Position = new MapCoordinates(position, tabletopMap);
        }
    }
}