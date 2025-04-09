namespace Content.Goobstation.Server.Mimery;

[RegisterComponent]
public sealed partial class MimeryBookComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float LearnTime = 2;
}
