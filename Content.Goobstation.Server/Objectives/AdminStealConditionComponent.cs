namespace Content.Goobstation.Server.Objectives;

/// <summary>
/// Objective condition to steal one specific entity, designated by an admin.
/// The objective is complete while the entity is carried by (or inside something carried by) the mind's body,
/// is being pulled by it, or is within range of a steal area (e.g. a thief beacon) the mind has linked.
/// Title, description and icon are filled in from the target by <see cref="AdminStealConditionSystem"/>.
/// </summary>
[RegisterComponent, Access(typeof(AdminStealConditionSystem))]
public sealed partial class AdminStealConditionComponent : Component
{
    /// <summary>
    /// The entity that has to be stolen.
    /// </summary>
    [DataField]
    public EntityUid? Target;

    /// <summary>
    /// Whether the target also counts when it is inside a steal area owned by the mind, like normal steal objectives.
    /// </summary>
    [DataField]
    public bool CheckStealAreas = true;
}
