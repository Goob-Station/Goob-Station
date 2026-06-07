using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SummonPortalComponent : Component
{
    /// <summary>
    /// How many portals are currently active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentActivePortals;

    /// <summary>
    /// The currently active portal spawned by this entity, if any.
    /// </summary>
    [ViewVariables]
    public EntityUid? CurrentPortal;

    /// <summary>
    /// How many portals the wraith is allowed to have.
    /// </summary>
    [DataField]
    public int PortalLimit = 1;

    /// <summary>
    /// The prototype ID for the void portal.
    /// </summary>
    [DataField]
    public EntProtoId RitualCircle = "RitualCircle4";

}
