using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Disease;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DiseaseComponent : Component
{
    /// <summary>
    /// The entity with <see cref="DiseaseCarrierComponent"/> that has this disease
    /// </summary>
    public EntityUid? Owner;

    /// <summary>
    /// How much this disease mutates on spread
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MutationRate = 0.2f;

    /// <summary>
    /// Affects mutation of mutation rate
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MutationMutationCoefficient = 1f;

    /// <summary>
    /// On spread, complexity may go up or down by <see cref="BaseMutationRate"/> multiplied by this
    /// </summary>
    [DataField, AutoNetworkedField]
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
}
