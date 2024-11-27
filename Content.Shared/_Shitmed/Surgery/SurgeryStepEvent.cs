namespace Content.Shared._Shitmed.Medical.Surgery;

/// <summary>
///     Raised on the step entity and the user after doing a step.
/// </summary>
[ByRefEvent]
public record struct SurgeryStepEvent(EntityUid User, EntityUid Body, EntityUid Part, List<EntityUid> Tools, EntityUid Surgery, EntityUid Step, bool Complete);
