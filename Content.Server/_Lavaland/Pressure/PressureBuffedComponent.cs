using System.Numerics;
using Content.Shared.Atmos;

namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureBuffedComponent : Component
{
    [DataField]
    public Vector2 RequiredPressure = new(0, Atmospherics.OneAtmosphere * 0.5f);

    [DataField]
    public float AppliedModifier = 1.5f;
}
