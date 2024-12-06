using Content.Shared.Atmos;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.GameStates;

namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureBuffedComponent : Component
{
    // why is minmax still an integer???
    [DataField] public MinMax RequiredPressure = new MinMax(0, (int) (Atmospherics.OneAtmosphere / 2));
    [DataField] public float AppliedModifier = 2f;
}
