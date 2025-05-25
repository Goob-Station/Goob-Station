// SPDX-FileCopyrightText: 2023 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Chemistry.Components.DeleteOnSolutionEmptyComponent
{
    /// <summary>
    /// Component that removes an item when a specific solution in it becomes empty.
    /// </summary>
    [RegisterComponent]
    public sealed partial class DeleteOnSolutionEmptyComponent : Component
    {
        /// <summary>
        /// The name of the solution of which to check emptiness
        /// </summary>
        [DataField("solution")]
        public string Solution = string.Empty;
    }
}