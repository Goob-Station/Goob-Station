using Content.Shared.Atmos;
namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureDamageChangeComponent : Component
{
    [DataField]
    public float LowerBound = 0;

    [DataField]
    public float UpperBound = Atmospherics.OneAtmosphere * 0.5f;

    [DataField]
    public bool ApplyWhenInRange = false;

    [DataField]
    public float AppliedModifier = 0.25f;

    [DataField]
    public bool ApplyToMelee = true;

    [DataField]
    public bool ApplyToProjectiles = true;
}
