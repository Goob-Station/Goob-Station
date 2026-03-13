using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Raptors.Genetics
{
    /// <summary>
    /// Runtime genetic modifiers inherited from parents.
    /// These modify base stats from the prototype.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class RaptorGenes
    {
        /// <summary>
        /// Attack modifier inherited from parents.
        /// </summary>
        public float AttackModifier = 1f;

        /// <summary>
        /// Health modifier inherited from parents.
        /// </summary>
        public float HealthModifier = 1f;

        /// <summary>
        /// Growth speed modifier.
        /// </summary>
        public float GrowthModifier = 1f;

        /// <summary>
        /// Ability modifier (used for healing, mining, etc).
        /// </summary>
        public float AbilityModifier = 1f;

        /// <summary>
        /// Personality traits inherited or randomly assigned.
        /// </summary>
        public HashSet<RaptorTrait> Traits = new();
    }
}
