using Robust.Shared.Prototypes;

namespace Content.Shared.Raptors.Prototypes
{
    /// <summary>
    /// Defines a raptor breed.
    /// Static configuration loaded from YAML.
    /// </summary>
    [Prototype("raptor")]
    public sealed class RaptorPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        /// <summary>
        /// Color identifier.
        /// </summary>
        [DataField]
        public string Color = "red";

        /// <summary>
        /// Base combat stats.
        /// </summary>
        [DataField]
        public float MaxHealth = 100;

        [DataField]
        public float MeleeDamageLower = 5;

        [DataField]
        public float MeleeDamageUpper = 10;

        /// <summary>
        /// Special abilities.
        /// </summary>
        [DataField]
        public bool Healer;

        [DataField]
        public bool Miner;

        [DataField]
        public bool Storage;

        [DataField]
        public bool MilkProducer;
    }
}
