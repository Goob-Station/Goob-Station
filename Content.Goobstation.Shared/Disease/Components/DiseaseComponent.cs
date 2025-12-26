using Content.Goobstation.Shared.Disease.Systems;
using Content.Shared.Random;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedDiseaseSystem), Other = AccessPermissions.ReadExecute)] // if the system's methods don't let you do something you want, add a method for it
[EntityCategory("Diseases")]
public sealed partial class DiseaseComponent : Component
{
    /// <summary>
    /// The effects this disease has
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Effects = [];

    /// <summary>
    /// Current strength of the organism's immunity against this disease
    /// Raises according to the organism's immunity gain rate
    /// Can at most reach 1
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ImmunityProgress;

    /// <summary>
    /// Determines current strength of disease
    /// Lowered by <see cref="ImmunityProgress"/> multiplied by the organism's immune power, per second
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InfectionProgress;

    /// <summary>
    /// Dictionary of effects to add on component startup to their respective severities
    /// </summary>
    [DataField("effects")]
    public Dictionary<EntProtoId, float> StartingEffects = new();

    /// <summary>
    /// How much to increase <see cref="InfectionProgress"/> per second
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InfectionRate = 0.01f;

    /// <summary>
    /// How much this disease mutates on spread
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MutationRate = 0.2f;

    /// <summary>
    /// Immunity gained against this disease is multiplied by this number
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ImmunityGainRate = 1f;

    /// <summary>
    /// Affects mutation of mutation rate
    /// </summary>
    [DataField]
    public float MutationMutationCoefficient = 1f;

    /// <summary>
    /// Affects mutation of immunity gain
    /// </summary>
    [DataField]
    public float ImmunityGainMutationCoefficient = 0.5f;

    /// <summary>
    /// Affects mutation of infection rate
    /// </summary>
    [DataField]
    public float InfectionRateMutationCoefficient = 0.5f;

    /// <summary>
    /// Affects mutation of complexity
    /// </summary>
    [DataField]
    public float ComplexityMutationCoefficient = 1f;

    /// <summary>
    /// Affects chance of severity of any given effect mutating
    /// </summary>
    [DataField]
    public float SeverityMutationCoefficient = 2f;

    /// <summary>
    /// Affects chance an effect is added or removed during mutation
    /// </summary>
    [DataField]
    public float EffectMutationCoefficient = 1.5f;

    /// <summary>
    /// Affects chance genotype is mutated
    /// </summary>
    [DataField]
    public float GenotypeMutationCoefficient = 0.5f;

    /// <summary>
    /// Determines total amount of effects and their severity after a mutation
    /// Important to, say, prevent gigacancer that infects everyone, as the cancer would usually take up most of the complexity
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Complexity = 20f;

    /// <summary>
    /// You can't be sick with a disease with one genotype twice, this includes vaccines
    /// May mutate and then nobody will be immune to the new virus
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Genotype;

    /// <summary>
    /// Whether you can gain immunity to this genotype, set to false for cancer and similar
    /// Prevents the entity from obtaining immunity to this genotype, does nothing if said immunity already exists
    /// </summary>
    [DataField]
    public bool CanGainImmunity = true;

    /// <summary>
    /// Whether to, instead of normal growth, use <see cref="DeadInfectionRate"/> in dead entities
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AffectsDead;

    /// <summary>
    /// If <see cref="AffectsDead"/> is true, how to change infection progress per second in dead entities
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DeadInfectionRate = -0.01f;

    /// <summary>
    /// Determiens the effects this disease mutates
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> AvailableEffects = "DiseaseBehaviorsStandard";

    /// <summary>
    /// Type of this disease
    /// Affects meds needed to heal this
    /// </summary>
    [DataField]
    public ProtoId<DiseaseTypePrototype> DiseaseType = "Debug";
}
