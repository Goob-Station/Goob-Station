using Content.Shared._White.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Larva;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class XenomorphLarvaVictimComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public ProtoId<InfectionIconPrototype>? InfectedIcon;
}
