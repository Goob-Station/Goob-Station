using Robust.Shared.Audio;
using Robust.Shared.Containers;
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
    /// What color is the slime?
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color SlimeColor = Color.FromHex("#828282");

    /// <summary>
    /// What is the current slime's current breed?
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<BreedPrototype> Breed = "GreyMutation";

    /// <summary>
    /// If the mutation chance is met, what potential mutations are available?
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<BreedPrototype>> PotentialMutations = new();

    /// <summary>
    /// The stomach! Holds all consumed entities to be consumed.
    /// </summary>
    [DataField]
    public Container Stomach = new();

    /// <summary>
    /// How many entities the slime can digest at once.
    /// </summary>
    [DataField]
    public int MaxContainedEntities = 3;

    /// <summary>
    /// How long each entity is stunned for when removed from the stomach (Fuck you gus.)
    /// </summary>
    [DataField]
    public TimeSpan OnRemovalStunDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The entity which has tamed this slime.
    /// </summary>
    [DataField]
    public EntityUid? Tamer;

    /// <summary>
    /// The entity, if any, currently being consumed by the slime.
    /// </summary>
    [DataField]
    public EntityUid? LatchedTarget;

    /// <summary>
    /// The maximum amount of offspring produced by mitosis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxOffspring = 4;

    /// <summary>
    /// What is the chance of offspring mutating? (this is per/offspring)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MutationChance = 0.45f;

    /// <summary>
    /// What hunger threshold must be met for mitosis?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MitosisHunger = 200f;

    /// <summary>
    /// Should this slime be metallic? (Shader & logic still needs to be written)
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShouldBeMetallic; // this should prob be an enum

    /// <summary>
    /// What sound should we play when mitosis occurs?
    /// </summary>
    [DataField]
    public SoundPathSpecifier MitosisSound = new("/Audio/Effects/Fluids/splat.ogg");

    /// <summary>
    /// What sound should we play when the slime eats/latches.
    /// </summary>
    [DataField]
    public SoundPathSpecifier EatSound = new("/Audio/Voice/Talk/slime.ogg");



}
