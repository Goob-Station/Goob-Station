// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
    /// Default slime.
    /// </summary>
    [DataField]
    public EntProtoId DefaultSlimeProto = "MobSlimeXenobioBaby";

    /// <summary>
    /// What color is the slime?
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color SlimeColor = Color.FromHex("#FFFFFF");

    /// <summary>
    /// What is the current slime's current breed?
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<BreedPrototype> Breed = "GreyMutation";

    /// <summary>
    /// If the associated breed prototype cannot be found,
    /// it will use this extract as a fallback.
    /// </summary>
    [DataField]
    public EntProtoId DefaultExtract = "GreySlimeExtract";

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
    public int MaxContainedEntities = 1;

    /// <summary>
    /// How long each entity is stunned for when removed from the stomach (Fuck you gus.)
    /// </summary>
    [DataField]
    public TimeSpan OnRemovalStunDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long the do-after to start a latch is.
    /// </summary>
    [DataField]
    public TimeSpan LatchDoAfterDuration = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The entity which has tamed this slime.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Tamer;

    [DataField]
    public EntProtoId TameEffect = "EffectHearts";

    /// <summary>
    /// The entity, if any, currently being consumed by the slime.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? LatchedTarget;

    /// <summary>
    /// The maximum amount of offspring produced by mitosis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxOffspring = 4;

    /// <summary>
    /// How many extracts will be produced by this slime?
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ExtractsProduced = 1;

    /// <summary>
    /// What is the chance of offspring mutating? (this is per/offspring)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MutationChance = 0.45f;

    /// <summary>
    /// What hunger threshold must be met for mitosis?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MitosisHunger = 125f;

    /// <summary>
    /// How long in between each mitosis/breeding check?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// When is the next mitosis/breeding check?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan NextUpdateTime;

    /// <summary>
    /// What should the minimum difference be between the current hunger and the mitosis hunger
    /// before the entity starts to shake?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float JitterDifference = 25f;

    /// <summary>
    /// Should this slime have a shader?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShouldHaveShader;

    /// <summary>
    /// Which shader are we using?
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Shader;

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
