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
