using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Doodon.Components;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class DoodonStructureComponent : Component
{
    /// <summary>
    /// The townhall that this doodon structure is linked to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid LinkedTownhall = default!;
    /// <summary>
    /// The amount of resin needed to build this structure.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ResinCost = 1;

    [DataField, AutoNetworkedField]
    public bool IsLinkedToTownhall = false;

    [DataField, AutoNetworkedField]
    public bool IsValid = false;
}
