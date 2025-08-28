using Robust.Shared.Serialization;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Events;

[Serializable, NetSerializable]
public sealed class FightActionChangeEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public AttackStrategy FightAction { get; }
    public SpriteSpecifier Icon { get; }
    public FightActionChangeEvent(NetEntity uid, AttackStrategy fightAction, SpriteSpecifier icon)
    {
        Uid = uid;
        FightAction = fightAction;
        Icon = icon;
    }
}
