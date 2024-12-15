using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._EinsteinEngines.Silicon.WeldingHealing
{
    [RegisterComponent]
    public sealed partial class WeldingHealingComponent : Component
    {
        /// <summary>
        ///     All the damage to change information is stored in this <see cref="DamageSpecifier"/>.
        /// </summary>
        /// <remarks>
        ///     If this data-field is specified, it will change damage by this amount instead of setting all damage to 0.
        ///     in order to heal/repair the damage values have to be negative.
        /// </remarks>

        [DataField(required: true)]
        public DamageSpecifier Damage;

        [DataField(customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
        public string QualityNeeded = "Welding";

        /// <summary>
        ///     The fuel amount needed to repair physical related damage
        /// </summary>
        [DataField]
        public int FuelCost = 5;

        [DataField]
        public int DoAfterDelay = 3;

        /// <summary>
        ///     A multiplier that will be applied to the above if an entity is repairing themselves.
        /// </summary>
        [DataField]
        public float SelfHealPenalty = 3f;

        /// <summary>
        ///     Whether or not an entity is allowed to repair itself.
        /// </summary>
        [DataField]
        public bool AllowSelfHeal = true;

        [DataField(required: true)]
        public List<string> DamageContainers;
    }
}
