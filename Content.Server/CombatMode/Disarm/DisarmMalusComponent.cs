// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.CombatMode.Disarm
{
    /// <summary>
    /// Applies a malus to disarm attempts against this item.
    /// </summary>
    [RegisterComponent]
    public sealed partial class DisarmMalusComponent : Component
    {
        /// <summary>
        /// So, disarm chances are a % chance represented as a value between 0 and 1.
        /// This default would be a 30% penalty to that.
        /// </summary>
        [DataField("malus")]
        public float Malus = 0.3f;
    }
}