namespace Content.Goobstation.Shared.Voidwalker.TemporarilyDisableCollision;

[RegisterComponent]
public sealed partial class TemporarilyDisableCollisionComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan EndTime;
}
