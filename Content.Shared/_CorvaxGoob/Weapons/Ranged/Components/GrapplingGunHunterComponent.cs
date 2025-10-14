using System;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._CorvaxGoob.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrapplingGunHunterComponent : Component
{
    /// <summary>
    ///     How quickly the tether shortens while reeling in, in world units per second.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ReelRate = 2.5f;

    /// <summary>
    ///     Extra length added to the joint when first attached to avoid snapping from immediate tension.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float JointSlack = 0.2f;

    /// <summary>
    ///     Minimum distance the joint will allow between the gun and the hooked target.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float JointMinLength = 0.35f;

    /// <summary>
    ///     Allowed tolerance above the minimum joint length before the pull animation stops automatically.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PullStopTolerance = 0.05f;

    /// <summary>
    ///     Furthest distance the hook can travel before automatically retracting.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxRange = 12f;

    /// <summary>
    ///     Minimum valid distance for a hook latch; closer than this cancels the grapple.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MinRange = 4f;

    /// <summary>
    ///     Distance at which the reeling process automatically stops to prevent over-shooting the target.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ReelStopDistance = 4f;

    /// <summary>
    ///     Currently spawned projectile entity, if any.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Projectile;

    /// <summary>
    ///     Entity that is currently tethered to the grappling gun.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? HookedTarget;

    /// <summary>
    ///     Whether dropping below the wield requirement should automatically cancel the grapple.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RequireWieldedHands = true;

    /// <summary>
    ///     Whether a stun should be attempted on a target once the hook embeds.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ApplyStunOnAttach = false;

    /// <summary>
    ///     Duration of the stun applied when <see cref="ApplyStunOnAttach"/> is true.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadWrite), DataField("reeling"), AutoNetworkedField]
    public bool Reeling;

    [ViewVariables(VVAccess.ReadWrite), DataField("reelSound"), AutoNetworkedField]
    public SoundSpecifier? ReelSound = new SoundPathSpecifier("/Audio/Weapons/reel.ogg")
    {
        Params = AudioParams.Default.WithLoop(true)
    };

    [ViewVariables(VVAccess.ReadWrite), DataField("cycleSound"), AutoNetworkedField]
    public SoundSpecifier? CycleSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/kinetic_reload.ogg");

    /// <summary>
    ///     Rope sprite used by the joint visualizer while the hook is active.
    /// </summary>
    [DataField]
    public SpriteSpecifier RopeSprite =
        new SpriteSpecifier.Rsi(new ResPath("_CorvaxGoob/Objects/Weapons/Guns/Launchers/grappling_gun.rsi"), "rope");

    /// <summary>
    ///     Currently playing reel audio stream entity, if any.
    /// </summary>
    public EntityUid? Stream;
}
