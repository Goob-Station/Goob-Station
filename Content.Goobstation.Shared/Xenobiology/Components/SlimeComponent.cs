using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// Stores important information about slimes.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlimeComponent : Component
{
    /// <summary>
    /// What is the current slime's current breed?
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<BreedPrototype> Breed = "GreyMutation";

    /// <summary>
    /// How many offspring should be produced by mitosis?
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Offspring = 4;

    /// <summary>
    /// What is the chance of offspring mutating? (this is per/offspring)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MutationChance = 0.45f;

    /// <summary>
    /// If the mutation chance is met, what potential mutations are available?
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<BreedPrototype>> PotentialMutations = new();

    /// <summary>
    /// What hunger threshold must be met for mitosis?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MitosisHunger = 200f;

    /// <summary>
    /// What color is the slime?
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color SlimeColor = Color.FromHex("#828282");

    [DataField]
    public SoundPathSpecifier SquishSound = new("/Audio/_EinsteinEngines/Voice/Slime/slime_squish.ogg");

}
