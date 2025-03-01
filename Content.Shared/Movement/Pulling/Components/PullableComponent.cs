using Content.Shared.Alert;
using Content.Shared.Movement.Pulling.Systems; // Goobstation
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Movement.Pulling.Components;

/// <summary>
/// Specifies an entity as being pullable by an entity with <see cref="PullerComponent"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(Systems.PullingSystem))]
public sealed partial class PullableComponent : Component
{
    /// <summary>
    /// The current entity pulling this component.
    /// </summary>
    [AutoNetworkedField, DataField]
    public EntityUid? Puller;

    /// <summary>
    /// The pull joint.
    /// </summary>
    [AutoNetworkedField, DataField]
    public string? PullJointId;

    public bool BeingPulled => Puller != null;

    /// <summary>
    /// If the physics component has FixedRotation should we keep it upon being pulled
    /// </summary>
    [Access(typeof(Systems.PullingSystem), Other = AccessPermissions.ReadExecute)]
    [ViewVariables(VVAccess.ReadWrite), DataField("fixedRotation")]
    public bool FixedRotationOnPull;

    /// <summary>
    /// What the pullable's fixedrotation was set to before being pulled.
    /// </summary>
    [Access(typeof(Systems.PullingSystem), Other = AccessPermissions.ReadExecute)]
    [AutoNetworkedField, DataField]
    public bool PrevFixedRotation;

    // Goobstation start
    // Added Grab variables


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
    public float GrabEscapeChance = 1f;

    [DataField]
    public ProtoId<AlertPrototype> PulledAlert = "Pulled";

    [AutoNetworkedField]
    public TimeSpan NextEscapeAttempt = TimeSpan.Zero;
    // Goobstation end
}

public sealed partial class StopBeingPulledAlertEvent : BaseAlertEvent;
