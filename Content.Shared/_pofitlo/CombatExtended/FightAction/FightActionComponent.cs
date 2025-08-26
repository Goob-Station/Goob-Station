using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robust.Shared.GameStates;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Content.Shared.Dataset;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared._pofitlo.CombatExtended.FightAction;

namespace Content.Shared._pofitlo.CombatExtended.FightAction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class FightActionComponent : Component
{

    [DataField, AutoNetworkedField]
    public List<ProtoId<FightActionPrototype>> AvailableActions = new();

    [DataField, AutoNetworkedField]
    public AttackStrategy Strategy = AttackStrategy.Punch;
}
