using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Xenobiology.Components;

/// <summary>
/// Stores important information about slimes.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlimeComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<BreedPrototype> Breed = "GreyMutation";

    [DataField, AutoNetworkedField]
    public int Offspring = 4;

    [DataField, AutoNetworkedField]
    public float MutationChance = 0.45f;

    [DataField, AutoNetworkedField]
    public HashSet<BreedPrototype> PotentialMutations { get; set; } = new HashSet<BreedPrototype>();

    [DataField, AutoNetworkedField]
    public float MitosisHunger = 20f;

}
