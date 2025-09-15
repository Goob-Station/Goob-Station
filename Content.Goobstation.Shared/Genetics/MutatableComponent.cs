using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Genetics;

/// <summary>
/// Allows an entity to have mutations.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(MutationSystem))]
[AutoGenerateComponentState]
public sealed partial class MutatableComponent : Component
{
    /// <summary>
    /// Currently active mutations.
    /// Can be set in YML for e.g. default species-specific mutations.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<MutationPrototype>> Mutations = new();

    /// <summary>
    /// Dormant mutations that can be added with a Activator.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<MutationPrototype>> Dormant = new();

    /// <summary>
    /// How much instability you have from mutations.
    /// Once this reaches <see cref="MaxInstability"/> it's joever.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalInstability;
}
