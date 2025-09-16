using Robust.Shared.Serialization;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Events;

[Serializable, NetSerializable]
public sealed class FightActionChangeEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public AttackStrategy FightAction { get; }
    public FightActionChangeEvent(NetEntity uid, AttackStrategy fightAction)
    {
        Uid = uid;
        FightAction = fightAction;
    }
}

[Serializable, NetSerializable]
public sealed class TailLightAttackEvent : AttackEvent
{
    public readonly NetEntity? Target;
    public readonly NetEntity Weapon;

    public TailLightAttackEvent(NetEntity? target, NetEntity weapon, NetCoordinates coordinates) : base(coordinates)
    {
        Target = target;
        Weapon = weapon;
    }
}
