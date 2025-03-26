using Content.Shared.Atmos;

namespace Content.Server._Goobstation.Temperature;

/// <summary>
/// Kills an entity when its temperature goes over a threshold.
/// </summary>
[RegisterComponent, Access(typeof(KillOnOverheatSystem))]
public sealed partial class KillOnOverheatComponent : Component
{
    [DataField]
    public float OverheatThreshold = Atmospherics.T0C + 110f;

    [DataField]
    public LocId OverheatPopup = "ipc-overheat-popup";
}
