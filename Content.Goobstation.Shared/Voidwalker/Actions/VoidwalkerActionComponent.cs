namespace Content.Goobstation.Shared.Voidwalker.Actions;

[RegisterComponent]
public sealed partial class VoidwalkerActionComponent : Component
{
    /// <summary>
    /// Whether this action requires the voidwalker to be in space to use it.
    /// </summary>
    [DataField]
    public bool RequireInSpace = true;
}
