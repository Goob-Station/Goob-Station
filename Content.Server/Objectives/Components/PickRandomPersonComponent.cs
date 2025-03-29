namespace Content.Server.Objectives.Components;

/// <summary>
/// Sets the target for <see cref="TargetObjectiveComponent"/> to a random person.
/// </summary>
[RegisterComponent]
public sealed partial class PickRandomPersonComponent : Component
{
    [DataField]
    public bool NeedsOrganic; // Goobstation: Only pick non-silicon players.
}
