using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Content.Shared._pofitlo.CombatExtended.FightAction;


namespace Content.Server._pofitlo.CombatExtended.FightAction;

public sealed class FightActionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FightActionChangeEvent>(OnFightActionChange);
    }

    private void OnFightActionChange(FightActionChangeEvent message, EntitySessionEventArgs args)
    {
        if (!TryComp<FightActionComponent>(GetEntity(message.Uid), out var fightActionComp))
            return;

        fightActionComp.Strategy = message.FightAction;
        DirtyField(GetEntity(message.Uid), fightActionComp, nameof(FightActionComponent.Strategy));
    }
}
