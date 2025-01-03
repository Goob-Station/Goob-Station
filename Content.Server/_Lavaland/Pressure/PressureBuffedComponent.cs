using System.Numerics;
using Content.Shared.Atmos;

namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureBuffedComponent : Component
{
    [DataField]
    public Vector2 RequiredPressure = new(0, Atmospherics.OneAtmosphere / 2);

    [DataField]
    public float AppliedModifier = 2f;
}
