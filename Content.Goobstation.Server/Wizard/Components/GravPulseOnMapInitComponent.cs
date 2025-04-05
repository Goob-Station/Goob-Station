namespace Content.Goobstation.Server.Wizard.Components;

[RegisterComponent]
public sealed partial class GravPulseOnMapInitComponent : Component
{
    [DataField]
    public float MaxRange = 1f;

    [DataField]
    public float MinRange;

    [DataField]
    public float BaseRadialAcceleration;

    [DataField]
    public float BaseTangentialAcceleration;
}
