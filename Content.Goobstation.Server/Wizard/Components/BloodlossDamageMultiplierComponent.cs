namespace Content.Goobstation.Server.Wizard.Components;

[RegisterComponent]
public sealed partial class BloodlossDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;
}
