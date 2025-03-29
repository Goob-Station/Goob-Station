namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class BloodlossDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;
}
