using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery;

/// <summary>
///     Raised on the step entity and the user after doing a step.
/// </summary>
[ByRefEvent]
public record struct SurgeryStepEvent(EntityUid User, EntityUid Body, EntityUid Part, List<EntityUid> Tools, EntityUid Surgery, EntityUid Step, bool Complete);

/// <summary>
/// Raised on the user after failing to do a step for any reason.
/// </summary>
[ByRefEvent]
public record struct SurgeryStepFailedEvent(EntityUid User, EntityUid Body, EntProtoId SurgeryId, EntProtoId StepId);
