using Robust.Shared.GameStates; // Goobstation - Ling absorb biomatter ability
using Robust.Shared.Serialization;

namespace Content.Shared.Nutrition.Components
{
    // TODO: Remove maybe? Add visualizer for food
    [Serializable, NetSerializable]
    public enum FoodVisuals : byte
    {
        Visual,
        MaxUses,
    }

    [Serializable, NetSerializable]
    public enum OpenableVisuals : byte
    {
        Opened,
        Layer
    }

    [Serializable, NetSerializable]
    public enum SealableVisuals : byte
    {
        Sealed,
        Layer,
    }

    // Goobstation - Ling absorb biomatter ability
    public abstract partial class SharedFoodComponent : Component
    {
        /// <summary>
        ///     If this is set to true, food can only be eaten if you have a stomach with a
        ///     <see cref="StomachComponent.SpecialDigestible"/> that includes this entity in its whitelist,
        ///     rather than just being digestible by anything that can eat food.
        ///     Whitelist the food component to allow eating of normal food.
        /// </summary>
        [DataField]
        public bool RequiresSpecialDigestion;
    }
}
