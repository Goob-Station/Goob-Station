using Content.Shared._pofitlo.CombatExtended.FightAction.AttackStrategySystems;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using System.Numerics;
using Content.Client._pofitlo.CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Client._pofitlo.CombatExtended;


namespace Content.Client._pofitlo.CombatExtended.AttackStrategySystems;

public sealed class TailAttackSystem : SharedTailAttackSystem
{
    [Dependency] private readonly CombatStrategyAnimationSystem _combatAnimation = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void DoLunge(EntityUid user, EntityUid weapon, Vector2 localPos, string? animation, Angle spriteRotation, bool flippedAnimation, bool predicted = true)
    {
        if (!TryComp(user, out FightActionComponent? fightActionComponent) && fightActionComponent == null)
        {
            return;
        }

        _combatAnimation.DoCombatStrategyAnimation(
            user,
            weapon,
            localPos,
            animation,
            spriteRotation,
            flippedAnimation,
            fightActionComponent,
            predicted);
    }

    public void DoTailAttackWithPrototype(EntityUid user, EntityUid weapon, Angle angle, Vector2 localPos, Angle spriteRotation, bool flippedAnimation)
    {
        var tempFightAction = new FightActionComponent
        {
            Strategy = AttackStrategy.TailAttack,
            CombatAnimationPrototype = "TailAttackAnimation"
        };
        _combatAnimation.DoCombatStrategyAnimation(
            user,
            weapon,
            localPos,
            "WeaponArcSlash",
            spriteRotation,
            flippedAnimation,
            tempFightAction,
            true
        );
    }
}
