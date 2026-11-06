using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Seals the mech and adds its stored air to the cabin while installed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechAirTankModuleComponent : Component
{
    [DataField(required: true)]
    public GasMixture Air = new();

    [ViewVariables, AutoNetworkedField]
    public bool? PreviousAirtight;
}
