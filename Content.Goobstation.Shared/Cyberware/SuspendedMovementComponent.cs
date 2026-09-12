namespace Content.Goobstation.Shared.Cyberware;

/// <summary>
/// Just exists to record the cybernetics previous movespeed buff.
/// Only real user of this rn are the speed legs.
/// </summary>
[RegisterComponent]
public sealed partial class SuspendedMovementComponent : Component
{
    [DataField]
    public float WalkSpeed;

    [DataField]
    public float SprintSpeed;

    [DataField]
    public float Acceleration;
}
