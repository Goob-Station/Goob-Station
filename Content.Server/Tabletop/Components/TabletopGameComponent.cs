// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Server.Tabletop.Components
{
    /// <summary>
    /// A component that makes an object playable as a tabletop game.
    /// </summary>
    [RegisterComponent, Access(typeof(TabletopSystem))]
    public sealed partial class TabletopGameComponent : Component
    {
        /// <summary>
        /// The localized name of the board. Shown in the UI.
        /// </summary>
        [DataField]
        public LocId BoardName { get; private set; } = "tabletop-default-board-name";

        /// <summary>
        /// The type of method used to set up a tabletop.
        /// </summary>
        [DataField(required: true)]
        public TabletopSetup Setup { get; private set; } = new TabletopChessSetup();

        /// <summary>
        /// The size of the viewport being opened. Must match the board dimensions otherwise you'll get the space parallax (unless that's what you want).
        /// </summary>
        [DataField]
        public Vector2i Size { get; private set; } = (300, 300);

        /// <summary>
        /// The zoom of the viewport camera.
        /// </summary>
        [DataField]
        public Vector2 CameraZoom { get; private set; } = Vector2.One;

        /// <summary>
        /// The specific session of this tabletop.
        /// </summary>
        [ViewVariables]
        public TabletopSession? Session { get; set; } = null;

        /// <summary>
        /// How many holograms have been spawned onto this board.
        /// </summary>
        [ViewVariables]
        public int HologramsSpawned { get; set; } = 0;

        /// <summary>
        /// How many holograms are allowed to be spawned total by players.
        /// </summary>
        [ViewVariables]
        public int MaximumHologramsAllowed { get; set; } = 10;
    }
}