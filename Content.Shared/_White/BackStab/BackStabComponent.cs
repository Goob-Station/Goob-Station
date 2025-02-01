namespace Content.Shared._White.BackStab;

[RegisterComponent]
public sealed partial class BackStabComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DamageMultiplier = 2f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Angle Tolerance = Angle.FromDegrees(45d);
}
