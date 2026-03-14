using Content.Goobstation.Common.Grab;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.GrabIntent;

/// <summary>
/// Stores grab-specific state for entities that can be grabbed while pulled.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabbableComponent : Component
{
    [DataField]
    public Dictionary<GrabStage, short> PulledAlertAlertSeverity = new()
    {
        { GrabStage.No, 0 },
        { GrabStage.Soft, 1 },
        { GrabStage.Hard, 2 },
        { GrabStage.Suffocate, 3 },
    };

    [AutoNetworkedField, DataField]
    public GrabStage GrabStage = GrabStage.No;

    [AutoNetworkedField, DataField]
    public float EscapeAttemptModifier = 1f;

    [AutoNetworkedField, DataField]
    public float GrabEscapeChance = 1f;

    [DataField]
    public ProtoId<AlertPrototype> PulledAlert = "Pulled";

    [AutoNetworkedField]
    public TimeSpan NextEscapeAttempt = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public bool BeingTabled;

    [DataField]
    public float BasedTabledForceSpeed = 5f;

    [DataField]
    public float TabledStaminaDamage = 40f;

    [DataField]
    public float TabledDamage = 5f;

    [DataField]
    public float EscapeAttemptCooldown = 2f;

    [DataField]
    public float PostTabledDuration = 3f;
}
