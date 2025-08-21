using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robust.Shared.GameStates;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared._pofitlo.CombatExtended.FightAction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FightActionComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TargetBodyPart Target = TargetBodyPart.Chest;
}
