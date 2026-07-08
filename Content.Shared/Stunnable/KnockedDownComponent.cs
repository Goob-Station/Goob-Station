// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Stunnable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas:true), AutoGenerateComponentPause, Access(typeof(SharedStunSystem))]
public sealed partial class KnockedDownComponent : Component
{
    /// <summary>
    /// Game time that we can stand up.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// Should we try to stand up?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutoStand = true;

    /// <summary>
    /// The Standing Up DoAfter.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ushort? DoAfterId;

    /// <summary>
    /// Friction modifier for knocked down players.
    /// Makes them accelerate and deccelerate slower.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FrictionModifier = 1f; // Should add a friction modifier to slipping to compensate for this

    /// <summary>
    /// Modifier to the maximum movement speed of a knocked down mover.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpeedModifier = 1f;

    /// <summary>
    /// How long does it take us to get up?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan GetUpDoAfter = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Goobstation old WD crawling Datafield, currently used for EE interaction verbs.
    /// </summary>
    [DataField("helpInterval"), AutoNetworkedField]
    public float HelpInterval = 1f;

    /// <summary>
    /// Goobstation old WD crawling Datafield, currently used for EE interaction verbs.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float HelpTimer = 0f;

    /// <summary>
    /// Goobstation old stun Datafield, currently used for EE interaction verbs.
    /// </summary>
    [DataField("helpAttemptSound")]
    public SoundSpecifier StunAttemptSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    // Shitmed Change: ANNOYING WITH THE GOD DAMN PAIN PROCS.
    [DataField, AutoNetworkedField]
    public bool StandOnRemoval = true;
}
