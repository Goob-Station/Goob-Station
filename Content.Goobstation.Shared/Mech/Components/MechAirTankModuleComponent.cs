using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Fills the mech with a standard o2 mixture while installed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechAirTankModuleComponent : Component
{
    [DataField]
    public float Volume = 500f;

    [ViewVariables]
    public GasMixture? Air;

    [ViewVariables, AutoNetworkedField]
    public bool? PreviousAirtight;
}
