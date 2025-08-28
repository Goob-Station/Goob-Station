using Robust.Shared.Serialization;

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
