// SPDX-FileCopyrightText: 2022 Jessica M <jessica@jessicamaybe.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Nutrition.Components
{
    /// <summary>
    /// Component that tags solution containers as trash when their contents have been emptied.
    /// Used for things like used ketchup packets or used syringes.
    /// </summary>
    [RegisterComponent]
    public sealed partial class TrashOnSolutionEmptyComponent : Component
    {
        /// <summary>
        /// The name of the solution of which to check emptiness
        /// </summary>
        [DataField("solution")]
        public string Solution { get; set; } = string.Empty;
    }
}