using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MassMushroomOrganismComponent : Component
{
    [DataField]
    public bool IsActive;

    [DataField, AutoNetworkedField]
    public EntityUid? Host;

    [DataField]
    public EntProtoId Action = "ActionFungalGrowth";
}
