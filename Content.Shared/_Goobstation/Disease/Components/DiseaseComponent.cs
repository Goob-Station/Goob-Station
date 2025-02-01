using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System;

namespace Content.Shared.Disease;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DiseaseComponent : Component
{
    // <state>

    /// <summary>
    /// The effects this disease has
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Effects = new();

    /// <summary>
    /// Current strength of the organism's immunity against this disease
    /// Raises according to the organism's immunity gain rate
    /// Can at most reach 1
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ImmunityProgress = 0f;

    /// <summary>
    /// Determines current strength of disease
    /// Lowered by <see cref="ImmunityProgress"/> multiplied by the organism's immune power, per second
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InfectionProgress = 0f;

    // </state>

    // <parameters>

    /// <summary>
    /// Effects to add on component startup
    /// </summary>
    [DataField("effects")]
    public List<EntProtoId> StartingEffects = new();

    /// <summary>
    /// After how much time to activate the disease effects.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan IncubationTime = TimeSpan.FromSeconds(30);

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

    // NO magic constants, not allowed

    /// <summary>
    /// Affects mutation of mutation rate
    /// </summary>
    [DataField]
    public float MutationMutationCoefficient = 1f;

    /// <summary>
    /// Affects mutation of immune resistance
    /// </summary>
    [DataField]
    public float ImmuneResistanceMutationCoefficient = 1f;

    /// <summary>
    /// On mutation, infection rate may go up or down by <see cref="BaseMutationRate"/> multiplied by this
    /// </summary>
    [DataField]
    public float InfectionRateMutationCoefficient = 0.005f;

    /// <summary>
    /// On mutation, complexity may go up or down by <see cref="BaseMutationRate"/> multiplied by this
    /// </summary>
    [DataField]
    public float ComplexityMutationCoefficient = 7f;

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
    public int Genotype = 0;

    // </parameters>
}
