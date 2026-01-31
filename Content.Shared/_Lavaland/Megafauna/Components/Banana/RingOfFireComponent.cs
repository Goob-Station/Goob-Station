using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components.Banana;

/// <summary>
/// Component that handles creating a ring of fire around the entity by spawning multiple rotating prototypes with DamageOnCollide.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RingOfFireComponent : Component
{
    /// <summary>
    /// How far the ring goes before it stops growing.
    /// </summary>
    [DataField]
    public float RingDistance = 2f;

    /// <summary>
    /// How quickly the skulls grow in distance from their starting point.
    /// </summary>
    [DataField]
    public float GrowSpeed = 1f;

    /// <summary>
    /// How many skulls to spawn around the entity.
    /// </summary>
    [DataField]
    public int NumberToSpawn = 7;

    /// <summary>
    /// So that other entities can use this action without using the boss's voiceline.
    /// </summary>
    [DataField]
    public bool ShouldSpeak;

    /// <summary>
    /// What the entity says when using this action.
    /// </summary>
    [DataField]
    public LocId Speech = "childish-oni-ring";

    /// <summary>
    /// Sound played when the action is used.
    /// </summary>
    [DataField]
    public SoundSpecifier? ActionSound = new SoundPathSpecifier("/Audio/Effects/fire.ogg");

    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId SkullPrototype = "BananaSmallSkullTemporary";
}
