using Content.Shared._pofitlo.CombatExtended.FightAction.AttackStrategySystems;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using System.Numerics;
using Content.Client._pofitlo.CombatExtended;
using Robust.Shared.Prototypes;


namespace Content.Client._pofitlo.CombatExtended.AttackStrategySystems;

public sealed class TailAttackSystem : SharedTailAttackSystem
{
    [Dependency] private readonly CombatStrategyAnimationSystem _combatAnimation = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void DoLunge(EntityUid user, EntityUid weapon, Vector2 localPos, string? animation, Angle spriteRotation, bool flippedAnimation, ProtoId<CombatAnimationPrototype>? combatAnimProto, bool predicted = true)
    {
        if (!TryComp(user, out FightActionComponent? fightActionComponent))
            return;

        _combatAnimation.DoCombatStrategyAnimation(
            user,
            weapon,
            localPos,
            animation,
            spriteRotation,
            flippedAnimation,
            fightActionComponent,
            combatAnimProto,
            predicted);
    }
}
