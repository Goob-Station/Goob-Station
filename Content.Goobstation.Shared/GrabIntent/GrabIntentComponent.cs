using Content.Goobstation.Common.Grab;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.GrabIntent;

/// <summary>
/// Stores grab-specific state for entities that can pull and escalate grabs.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class GrabIntentComponent : Component
{
    [DataField]
    public Dictionary<GrabStage, short> PullingAlertSeverity = new()
    {
        { GrabStage.No, 0 },
        { GrabStage.Soft, 1 },
        { GrabStage.Hard, 2 },
        { GrabStage.Suffocate, 3 },
    };

    [DataField, AutoNetworkedField]
    public GrabStage GrabStage = GrabStage.No;

    [DataField, AutoNetworkedField]
    public GrabStageDirection GrabStageDirection = GrabStageDirection.Increase;

    [AutoNetworkedField]
    public TimeSpan NextStageChange;

    [DataField]
    public TimeSpan StageChangeCooldown = TimeSpan.FromSeconds(1f);

    [DataField]
    public float DownedEscapeChanceMultiplier = 0.5f;

    [DataField]
    public Dictionary<GrabStage, float> EscapeChances = new()
    {
        { GrabStage.No, 1f },
        { GrabStage.Soft, 1f },
        { GrabStage.Hard, 0.6f },
        { GrabStage.Suffocate, 0.2f },
    };

    [DataField]
    public float SuffocateGrabStaminaDamage = 10f;

    [DataField]
    public float GrabThrowDamageModifier = 2f;

    [DataField]
    public FixedPoint2 GrabThrowDamage = 5;

    [DataField]
    public string GrabThrowDamageType = "Blunt";

    [ViewVariables]
    public readonly Dictionary<GrabStage, int> GrabVirtualItemStageCount = new()
    {
        { GrabStage.Suffocate, 1 },
    };

    [DataField]
    public float GrabThrownSpeed = 7f;

    [DataField]
    public float ThrowingDistance = 4f;

    [DataField]
    public float SoftGrabSpeedModifier = 0.9f;

    [DataField]
    public float HardGrabSpeedModifier = 0.7f;

    [DataField]
    public float ChokeGrabSpeedModifier = 0.4f;

    [NonSerialized]
    public readonly SoundPathSpecifier GrabSoundEffect = new("/Audio/Effects/thudswoosh.ogg");

    #region Table Slamming
    [DataField]
    public float TableSlamCooldown = 3f;

    [DataField]
    public float TableSlamRange = 2f;

    [DataField]
    public GrabStage TableSlamRequiredStage = GrabStage.Hard;
    #endregion
}
