using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared.Atmos.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared._pofitlo.CombatExtended.FightAction;

namespace Content.Shared._pofitlo.CombatExtended.FightAction;

public abstract class SharedFightActionSystem : EntitySystem
{

    public bool FightActionHasHigherPriority(EntityUid user) // _pofitlo // TODO переписать и переименовать
    {
        if (!TryComp<FightActionComponent>(user, out var fightActionComp))
            return false;

        return fightActionComp.HasHigherPriorityThanWeapons;
    }
}
