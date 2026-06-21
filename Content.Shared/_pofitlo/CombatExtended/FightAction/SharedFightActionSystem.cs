namespace Content.Shared._pofitlo.CombatExtended.FightAction;

public abstract class SharedFightActionSystem : EntitySystem
{

    public bool FightActionHasHigherPriority(EntityUid user)
    {
        if (!TryComp<FightActionComponent>(user, out var fightActionComp))
            return false;

        return fightActionComp.HasHigherPriorityThanWeapons;
    }
}
