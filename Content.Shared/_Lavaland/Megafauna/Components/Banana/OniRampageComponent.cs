using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components.Banana;

/// <summary>
/// Component that handles dashing towards a target and then creating a ring of fire where you land.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class OniRampageComponent : Component
{
    /// <summary>
    /// How far you will jump (in tiles).
    /// </summary>
    [DataField]
    public float JumpDistance = 6f;

    /// <summary>
    /// Basic “throwing” speed for TryThrow method.
    /// </summary>
    [DataField]
    public float JumpThrowSpeed = 8f;

    /// <summary>
    /// Whether this entity is mid-leap or not. Used to prevent collisions being accidentally triggered outside of the leap.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsLeaping;

    /// <summary>
    /// The range of the damage on landing the leap.
    /// </summary>
    [DataField]
    public float LandDamageRange = 2f;

    /// <summary>
    /// The damage it does if you get caught in the range.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// So that other entities can use this action without using the boss's voiceline.
    /// </summary>
    [DataField]
    public bool ShouldSpeak;

    /// <summary>
    /// What the entity says when using this action.
    /// </summary>
    [DataField]
    public LocId Speech = "childish-oni-rampage";

    [DataField]
    public SoundSpecifier? ShockwaveSound = new SoundCollectionSpecifier("ExplosionSmall");

    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId FirePrototype = "HereticFireAA";
}
