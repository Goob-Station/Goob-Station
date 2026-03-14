using Content.Goobstation.Shared.Raptors.Genetics;
using Content.Shared.Raptors.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Raptors.Components
{
    /// <summary>
    /// Core runtime state for a raptor entity.
    /// Holds dynamic values like happiness, growth, and genetics.
    /// Breed data comes from RaptorPrototype.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class RaptorComponent : Component
    {
        /// <summary>
        /// Breed definition (Red, Yellow, etc).
        /// </summary>
        [DataField, AutoNetworkedField]
        public ProtoId<RaptorPrototype> RaptorType = "Red";

        /// <summary>
        /// Genetic modifiers inherited from parents.
        /// </summary>
        [DataField, AutoNetworkedField]
        public RaptorGenes Genes = new();

        /// <summary>
        /// Whether the raptor is currently able to breed.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool BreedingMood;

        /// <summary>
        /// Cooldown preventing constant breeding.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float NextBreedTime;

        /// <summary>
        /// Happiness affects growth, combat bonuses, and milk production.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float Happiness;

        /// <summary>
        /// Parent prototypes used for breeding calculations.
        /// </summary>
        [DataField, AutoNetworkedField]
        public EntProtoId? Father;

        [DataField, AutoNetworkedField]
        public EntProtoId? Mother;
    }
}
