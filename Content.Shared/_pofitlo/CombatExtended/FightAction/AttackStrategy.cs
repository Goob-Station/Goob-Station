using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._pofitlo.CombatExtended.FightAction;

[Flags]
public enum AttackStrategy : ushort // TODO придумать что-то с именованием FightActionControl
{
    Punch = 1,
    TailAttack = 1 << 2,
}
