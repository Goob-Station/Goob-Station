// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Bible.Components
{
    /// <summary>
    /// This component is for the chaplain's familiars, and mostly
    /// used to track their current state and to give a component to check for
    /// if any special behavior is needed.
    /// </summary>
    [RegisterComponent]
    public sealed partial class FamiliarComponent : Component
    {
        /// <summary>
        /// The entity this familiar was summoned from.
        /// </summary>
        [ViewVariables]
        public EntityUid? Source = null;
    }
}