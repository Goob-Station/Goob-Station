namespace Content.Server.Ghost.Roles.Components;

public sealed partial class ToggleableGhostRoleComponent
{
    /// <summary>
    /// Goobstation - Whether the brain can be wiped via a verb.
    /// </summary>
    [DataField]
    public bool CanBeWiped = true;
}