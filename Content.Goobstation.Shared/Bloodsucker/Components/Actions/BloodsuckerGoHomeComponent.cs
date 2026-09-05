using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodsuckerGoHomeComponent : Component
{
    /// <summary>
    /// Total time before teleporting after activation.
    /// </summary>
    [DataField]
    public float TeleportDelay = 6f;

    /// <summary>
    /// Blood drained per second while active.
    /// </summary>
    [DataField]
    public float BloodDrainPerSecond = 2f;

    /// <summary>
    /// Range within which lights are flickered.
    /// </summary>
    [DataField]
    public float FlickerRange = 5f;

    /// <summary>
    /// Animals that can be spawned at the departure point.
    /// Weighted by value.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, int> SpawnMobs = new()
    {
        { "MobMouse", 3 },
        { "MobBat", 1 },
    };

    /// <summary>
    /// Whether the teleport sequence is currently running.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    /// <summary>
    /// Time elapsed since activation.
    /// </summary>
    [DataField]
    public float Elapsed;

    /// <summary>
    /// Whether the first flicker has already fired.
    /// </summary>
    [DataField]
    public bool FiredFlickerOne;

    /// <summary>
    /// Whether the second flicker has already fired.
    /// </summary>
    [DataField]
    public bool FiredFlickerTwo;

    [DataField]
    public SoundSpecifier? FlickerSound = new SoundPathSpecifier("/Audio/_Lavaland/heartbeat.ogg");

    [DataField]
    public SoundSpecifier? TeleportSound = new SoundPathSpecifier("/Audio/_Goobstation/Bloodsucker/Effects/summon_karp.ogg");

    #region Generic

    /// <summary>
    /// Blood cost deducted from the vampire's bloodstream to activate.
    /// </summary>
    [DataField]
    public float BloodCost = 100f;

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
