using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Counter;

/// <summary>
/// Counters anything that hits the user.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class CounterComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool DodgeProjectile;

    [DataField, AutoNetworkedField]
    public bool ProjectileOnly;

    [DataField, AutoNetworkedField]
    public bool MeleeOnly;

    [DataField, AutoNetworkedField]
    public bool CancelOnBuildup = true;

    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField, AutoNetworkedField]
    public float SpeedModifier = 1f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan ArmTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan BuildupEndTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan CounterEndTime;

    [DataField]
    public TimeSpan TriggerFlashTime = TimeSpan.FromSeconds(0.4);

    [DataField, AutoNetworkedField]
    public string? ArmedShader;

    [DataField, AutoNetworkedField]
    public string? BuildupShader;

    [DataField, AutoNetworkedField]
    public string? TriggeredShader;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? BuildupSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? ArmedSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? ArmedLoopSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? BeatSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? TriggerSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? FizzleSound;

    [DataField]
    public EntityUid? ArmedStream;

    [DataField]
    public int LastBeat = -1;

    [DataField]
    public int ShownPhase = -1;

    [DataField]
    public bool Ended;

    [DataField]
    public EntityUid? PendingMeleeAttacker;

    [DataField, AutoNetworkedField]
    public bool Triggered;
}
