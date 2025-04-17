using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Shared._Sunrise.AssaultOps.Icarus;

/// <summary>
/// Used for Icarus terminal activation
/// </summary>
[RegisterComponent]
public sealed partial class IcarusTerminalComponent : Component
{
    /// <summary>
    ///     Default fire timer value in seconds.
    /// </summary>
    [DataField("timer")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Timer = 25;

    /// <summary>
    ///     How long until the beam can arm again after fire.
    /// </summary>
    [DataField("cooldown")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Cooldown = 240;

    /// <summary>
    ///     Current status of a terminal.
    /// </summary>
    [ViewVariables]
    public IcarusTerminalStatus Status = IcarusTerminalStatus.AWAIT_DISKS;

    /// <summary>
    ///     Time until beam will be spawned in seconds.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float RemainingTime;

    /// <summary>
    ///     Time until beam cooldown will expire in seconds.
    /// </summary>
    [ViewVariables]
    public float CooldownTime;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keySlots")]
    public int KeySlots = 3;

    [ViewVariables]
    public Container KeyContainer = default!;
    public const string KeyContainerName = "key_slots";

    [ViewVariables]
    public float TimerRoundEnd = 1200;

    [DataField("activeGoldenEyeAlertSound")]
    public SoundSpecifier ActiveGoldenEyeAlertSound = new SoundPathSpecifier("/Audio/_Sunrise/AssaultOperatives/golden_eye_alarm.ogg");

    [DataField("alertSound")]
    public SoundSpecifier AlertSound = new SoundPathSpecifier("/Audio/_Sunrise/AssaultOperatives/icarus_alarm.ogg");

    [DataField("fireSound")]
    public SoundSpecifier FireSound = new SoundPathSpecifier("/Audio/_Sunrise/AssaultOperatives/sunbeam_fire.ogg");

    /// <summary>
    ///     Check if already notified about system authorization
    /// </summary>
    public bool AuthorizationNotified = false;
}
