using Content.Shared.Atmos;

namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureArmorChangeComponent : Component
{
    [DataField]
    public float LowerBound = Atmospherics.OneAtmosphere * 0.2f;

    [DataField]
    public float UpperBound = Atmospherics.OneAtmosphere * 0.5f;

    [DataField]
    public bool ApplyWhenInRange;

    [DataField]
    public float ExtraPenetrationModifier = 0.5f;
}
