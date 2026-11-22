using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Handles everything gang crates do
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class GangCrateComponent : Component
{
    /// <summary>
    /// How long the crate stays anchored after spawning
    [DataField]
    public TimeSpan AnchorDuration = TimeSpan.FromMinutes(3);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan AnchoredUntil = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan HalfThreshold = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LowThreshold = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan BlinkThreshold = TimeSpan.Zero;

    /// <summary>
    /// How many tiles away a gang locker must be for the crate to be unlockable.
    /// </summary>
    [DataField]
    public float LockerRange = 3f;

    [DataField, AutoNetworkedField]
    public GangCrateLightState LightState = GangCrateLightState.Full;

    /// <summary>
    /// Points awarded to every member of the gang whose locker is nearest when the crate is opened.
    /// </summary>
    [DataField]
    public int OpenReward = 3000;

    [DataField]
    public SoundSpecifier RewardSound = new SoundPathSpecifier("/Audio/_Goobstation/Gangs/cashout_success.ogg");

    [DataField]
    public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Effects/beep_landmine.ogg",
        AudioParams.Default.WithLoop(true).WithMaxDistance(5f));

    [DataField]
    public SoundSpecifier ReadySound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/ding.ogg",
        AudioParams.Default.WithMaxDistance(10f));

    [ViewVariables]
    public EntityUid? BlinkSoundEntity;

    [DataField, AutoNetworkedField]
    public bool RewardGiven;
}

public enum GangCrateLightState : byte
{
    Off,
    Blinking,
    Low,
    Half,
    Full,
}
