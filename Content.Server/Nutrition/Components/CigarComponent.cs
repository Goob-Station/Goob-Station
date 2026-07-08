// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Nutrition.EntitySystems;

namespace Content.Server.Nutrition.Components
{
    /// <summary>
    ///     A disposable, single-use smokable.
    /// </summary>
    [RegisterComponent, Access(typeof(SmokingSystem))]
    public sealed partial class CigarComponent : Component
    {
        /// <summary>
        ///     Goob - If a cigar can be ignited without a lighter by activating it
        /// </summary>
        [DataField("selfIgniting"), ViewVariables(VVAccess.ReadWrite)]
        public bool SelfIgniting { get; set; } = false;
    }
}
