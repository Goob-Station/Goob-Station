using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodsuckerOlfactionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? StopActionEntity;

    /// <summary>
    /// The entity currently being tracked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public NetEntity? TrackedTarget;

    /// <summary>
    /// Whether the tracking overlay and heartbeat are active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsTracking;

    /// <summary>
    /// How often the heartbeat/direction popup fires, in seconds.
    /// </summary>
    [DataField]
    public float HeartbeatInterval = 2f;

    [DataField]
    public float HeartbeatTimer;

    /// <summary>
    /// Scan range for blood puddles on activation.
    /// </summary>
    [DataField]
    public float ScanRange = 2f;

    [DataField]
    public SoundSpecifier HeartbeatSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Heretic/heartbeat.ogg");

    [DataField]
    public SoundSpecifier AcquireSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Heretic/heartbeat.ogg");

    #region Generic

    /// <summary>
    /// The current level of this action.
    /// </summary>
    public int ActionLevel = 1;

    /// <summary>
    /// The highest level this action can become.
    /// </summary>
    public int MaxLevel = 5;

    /// <summary>
    /// Blood cost deducted from the vampire's bloodstream to activate.
    /// </summary>
    [DataField]
    public float BloodCost = 0f;

    /// <summary>
    /// Humanity lost when activating this action.
    /// </summary>
    [DataField]
    public float HumanityCost = 0f;

    /// <summary>
    /// If true, this action cannot be started while the vampire is in frenzy.
    /// </summary>
    [DataField]
    public bool DisabledInFrenzy = false;

    #endregion
}
