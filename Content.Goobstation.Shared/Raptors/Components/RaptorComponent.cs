using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Raptors.Components
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class RaptorComponent : Component
    {
        #region Base Stats

        /// <summary>
        /// Maximum health of the raptor.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float MaxHealth;

        /// <summary>
        /// Current health of the raptor.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float CurrentHealth;

        /// <summary>
        /// Base movement speed of the raptor.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float Speed;

        /// <summary>
        /// Minimum melee damage this raptor deals.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float MeleeDamageLower;

        /// <summary>
        /// Maximum melee damage this raptor deals.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float MeleeDamageUpper;

        #endregion

        #region Growth & Breeding

        /// <summary>
        /// Can this raptor breed.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool CanBreed;

        /// <summary>
        /// The entity prototype the raptor will grow into.
        /// </summary>
        [DataField, AutoNetworkedField]
        public EntProtoId? GrowthPath;

        /// <summary>
        /// Progress towards growth (0.0 - 1.0).
        /// </summary>
        [DataField, AutoNetworkedField]
        public float GrowthProgress;

        /// <summary>
        /// Multiplier for growth speed.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float GrowthRate = 1.5f;

        #endregion

        #region Behavior Traits

        /// <summary>
        /// This raptor will care for babies if true.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool IsMotherly;

        /// <summary>
        /// This raptor will play with its owner if true.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool IsPlayful;

        /// <summary>
        /// This raptor will flee from danger if true.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool IsCoward;

        /// <summary>
        /// This raptor will attempt to heal other raptors if true.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool IsHealer;

        #endregion

        #region Mood & Happiness

        /// <summary>
        /// Current happiness level of the raptor (0-100).
        /// </summary>
        [DataField, AutoNetworkedField]
        public float Happiness;

        /// <summary>
        /// Current hunger level of the raptor (0-100).
        /// </summary>
        [DataField, AutoNetworkedField]
        public float Hunger;

        #endregion

        #region Genetics & Inheritance

        /// <summary>
        /// Attack stat modifier inherited from parents.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float AttackModifier;

        /// <summary>
        /// Health stat modifier inherited from parents.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float HealthModifier;

        /// <summary>
        /// List of traits inherited from parents.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> InheritedTraits = new();

        #endregion

        #region Appearance

        /// <summary>
        /// Color of the raptor, used for Dex and hybrids.
        /// </summary>
        [DataField, AutoNetworkedField]
        public string RaptorColor = "red";

        /// <summary>
        /// Gender of the raptor (optional, for Dex display).
        /// </summary>
        [DataField, AutoNetworkedField]
        public string Gender = "Unknown";

        /// <summary>
        /// Description for the raptor, used in Dex and UI tooltips.
        /// </summary>
        [DataField, AutoNetworkedField]
        public string DexDescription = "";

        #endregion

        #region Parentage

        /// <summary>
        /// Reference to the father raptor entity.
        /// </summary>
        [DataField, AutoNetworkedField]
        public EntProtoId? Father;

        /// <summary>
        /// Reference to the mother raptor entity.
        /// </summary>
        [DataField, AutoNetworkedField]
        public EntProtoId? Mother;

        #endregion
    }
}
