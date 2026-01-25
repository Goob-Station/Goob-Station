namespace Content.Goobstation.Server.Emp;

[RegisterComponent]
public sealed partial class EmpDisablingComponent : Component
{
    [DataField]
    public TimeSpan DisablingTime;
}
