using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Teleports an entity around an anchored coordinate, fades out their sprite as they teleport, and fades it back in.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FadingAnchoredTeleportComponent : Component
{
    /// <summary>
    /// The anchor entity after being spawned in, used to keep track of coordinates to spawn around.
    /// </summary>
    public EntityUid? AnchorEntity;

    /// <summary>
    /// Anchor prototype.
    /// </summary>
    [DataField]
    public EntProtoId? AnchorPrototype = "ORTAnchor";

    /// <summary>
    /// How far the entity can teleport away from the anchor.
    /// </summary>
    [DataField]
    public float TeleportDistance = 5f;

    /// <summary>
    /// How frequently it teleports.
    /// </summary>
    [DataField]
    public float TeleportDelay = 6f;

    /// <summary>
    /// If true, plays sound.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShouldPlaySound = true;

    /// <summary>
    /// The sound it makes.
    /// </summary>
    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/Shadowkin/futuristic-teleport.ogg");

    /// <summary>
    /// If true, the entity will quickly rush towards the location instead of instantly teleporting.
    /// If you set this to true, I HIGHLY recommend putting DamageOnCollide structural damage on your entity,
    /// or it will get stuck a lot.
    /// </summary>
    [DataField]
    public bool MoveInstead;

    /// <summary>
    /// Speed at which to move towards the coordinates.
    /// </summary>
    [DataField]
    public float MoveSpeed = 8f;

    public Vector2? MoveTarget;

    // I hate this but rule of cool beats being generic.
    [DataField]
    public ComponentRegistry? DashTrail;

    /// <summary>
    /// How long to fadeout for.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FadeOutTime = 0.75f;

    [AutoNetworkedField]
    public bool FadeOutStarted;

    [AutoNetworkedField]
    public bool FadeInStarted;

    [AutoNetworkedField]
    public float Accumulator;

    public const string AnimationKey = "fading_anchored_teleport";
}
