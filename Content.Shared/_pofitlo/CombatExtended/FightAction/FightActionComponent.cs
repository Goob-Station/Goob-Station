using Robust.Shared.GameStates;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._pofitlo.CombatExtended.FightAction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class FightActionComponent : Component
{

    [DataField, AutoNetworkedField]
    public List<ProtoId<FightActionPrototype>> AvailableActions = new();

    [DataField, AutoNetworkedField]
    public AttackStrategy Strategy;
}
