namespace Content.Server._Goobstation.Redial;

[RegisterComponent]
public sealed partial class RedialUserOnTriggerComponent : Component
{
    [DataField]
    public string Address = string.Empty;
}
