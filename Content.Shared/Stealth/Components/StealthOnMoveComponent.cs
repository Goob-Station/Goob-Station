// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Stealth.Components
{
    /// <summary>
    ///     When added to an entity with stealth component, this component will change the visibility
    ///     based on the entity's (lack of) movement.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState] // Goobstation
    public sealed partial class StealthOnMoveComponent : Component
    {
        /// <summary>
        /// Rate that effects how fast an entity's visibility passively changes.
        /// </summary>
        [DataField]
        [AutoNetworkedField] // Goobstation
        public float PassiveVisibilityRate = -0.15f;

        /// <summary>
        /// Rate for movement induced visibility changes. Scales with distance moved.
        /// </summary>
        [DataField]
        [AutoNetworkedField] // Goobstation
        public float MovementVisibilityRate = 0.2f;

        // <Goobstation> Goobstation - Proper invisibility
        /// <summary>
        /// How much to penalize minimum visibility depending on velocity.
        /// </summary>
        [DataField]
        [AutoNetworkedField] // Goobstation
        public float InvisibilityPenalty = 1f;

        /// <summary>
        /// Don't penalize minimum visibility beyond this amount.
        /// </summary>
        [DataField]
        [AutoNetworkedField] // Goobstation
        public float MaxInvisibilityPenalty = 0.5f;
        // </Goobstation>
    }
}