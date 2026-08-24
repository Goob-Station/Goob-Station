// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

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

        // Goobstation - Wait before stealth start accumulating 
        /// <summary>
        /// How long you shouldn't move to start accumulating stealth.
        /// </summary>
        [DataField]
        [AutoNetworkedField]
        public TimeSpan NoMoveTime = TimeSpan.Zero;

        // goob - break stealth on any movement
        [DataField]
        [AutoNetworkedField]
        public bool BreakOnMove;
        // </Goobstation>
    }
}
