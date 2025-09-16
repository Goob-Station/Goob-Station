using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared._pofitlo.CombatExtended.FightAction.AttackStrategySystems;
using System.Numerics;


namespace Content.Server._pofitlo.CombatExtended.FightAction.AttackStrategySystems;

public sealed class TailAttackSystem : SharedTailAttackSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void DoLunge(EntityUid user, EntityUid weapon, Vector2 localPos, string? animation, Angle spriteRotation, bool flippedAnimation, bool predicted = true)
    {
        int x = 0;
    }
}
