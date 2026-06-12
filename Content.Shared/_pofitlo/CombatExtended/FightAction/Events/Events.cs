using Robust.Shared.Serialization;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Events;

[Serializable, NetSerializable]
public sealed class FightActionChangeEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public AttackStrategy FightAction { get; }
    public bool HasHigherPriorityThanWeapons { get; }
    public ProtoId<FightActionMeleeParametersPrototype> FightActionMeleeParametersProto { get; }
    public ProtoId<CombatAnimationPrototype> CombatAnimationProto { get; }
    public ProtoId<CombatAnimationPrototype> AltCombatAnimationProto { get; }

    public FightActionChangeEvent(
        NetEntity uid,
        AttackStrategy fightAction,
        bool hasHigherPriorityThanWeapons,
        ProtoId<FightActionMeleeParametersPrototype> fightActionMeleeParametersProto,
        ProtoId<CombatAnimationPrototype> combatAnimationProto,
        ProtoId<CombatAnimationPrototype> altCombatAnimationProto)
    {
        Uid = uid;
        FightAction = fightAction;
        HasHigherPriorityThanWeapons = hasHigherPriorityThanWeapons;
        FightActionMeleeParametersProto = fightActionMeleeParametersProto;
        CombatAnimationProto = combatAnimationProto;
        AltCombatAnimationProto = altCombatAnimationProto;
    }
}

[Serializable, NetSerializable]
public sealed class TailMainAttackEvent : AttackEvent
{
    public readonly NetEntity Weapon;
    public List<NetEntity>? Entities;

    public TailMainAttackEvent(NetEntity weapon, List<NetEntity>? entities, NetCoordinates coordinates) : base(coordinates)
    {
        Weapon = weapon;
        Entities = entities;
    }
}

[Serializable, NetSerializable]
public sealed class TailAltAttackEvent : AttackEvent
{
    public readonly NetEntity? Target;
    public readonly NetEntity Weapon;

    public TailAltAttackEvent(NetEntity weapon, NetEntity? target, NetCoordinates coordinates) : base(coordinates)
    {
        Weapon = weapon;
        Target = target;
    }
}
