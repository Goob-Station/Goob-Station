using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Content.Shared._Shitmed.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Content.Shared.Mobs;


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
