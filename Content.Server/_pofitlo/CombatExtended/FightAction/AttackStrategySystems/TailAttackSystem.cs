using Content.Shared._pofitlo.CombatExtended.FightAction.AttackStrategySystems;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Server._pofitlo.CombatExtended.FightAction.AttackStrategySystems;

public sealed class TailAttackSystem : SharedTailAttackSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void DoLunge(EntityUid user, EntityUid weapon, Vector2 localPos, string? animation, Angle spriteRotation, bool flippedAnimation, ProtoId<CombatAnimationPrototype>? combatAnimProto, bool predicted = true)
    {
    }
}
