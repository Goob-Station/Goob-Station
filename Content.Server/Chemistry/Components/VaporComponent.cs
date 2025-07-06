// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Vince <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 nuke <47336974+nuke-makes-games@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 TekuNut <13456422+TekuNut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server.Chemistry.Components
{
    [RegisterComponent]
    public sealed partial class VaporComponent : Component
    {
        public const string SolutionName = "vapor";

        /// <summary>
        /// Stores data on the previously reacted tile. We only want to do reaction checks once per tile.
        /// </summary>
        [DataField]
        public TileRef? PreviousTileRef;

        /// <summary>
        /// Percentage of the reagent that is reacted with the TileReaction.
        /// <example>
        /// 0.5 = 50% of the reagent is reacted.
        /// </example>
        /// </summary>
        [DataField]
        public float TransferAmountPercentage;

        /// <summary>
        /// The minimum amount of the reagent that will be reacted with the TileReaction.
        /// We do this to prevent floating point issues. A reagent with a low percentage transfer amount will
        /// transfer 0.01~ forever and never get deleted.
        /// <remarks>Defaults to 0.05 if not defined, a good general value.</remarks>
        /// </summary>
        [DataField]
        public float MinimumTransferAmount = 0.05f;

        [DataField]
        public bool Active;
    }
}
