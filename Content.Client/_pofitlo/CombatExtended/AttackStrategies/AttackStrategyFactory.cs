using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared.Interaction;
using Robust.Client.GameObjects;
using Robust.Client.State;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Content.Client.Weapons.Melee;

namespace Content.Client._pofitlo.CombatExtended.AttackStrategies;

public sealed class AttackStrategyFactory
{
    private readonly Dictionary<AttackStrategy, IAttackStrategy> _cache;

    public AttackStrategyFactory(
        IStateManager stateManager,
        SharedInteractionSystem interaction,
        TransformSystem transform,
        INetManager netManager,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        MeleeWeaponSystem meleeWeaponSystem)
    {
        _cache = new()
        {
            [AttackStrategy.Punch] = new PunchAttackStrategy(stateManager, interaction, transform, netManager, entityManager, prototypeManager, meleeWeaponSystem),
            [AttackStrategy.TailAttack] = new TailAttackStrategy(stateManager, interaction, transform, netManager, entityManager, prototypeManager, meleeWeaponSystem),
        };
    }

    public IAttackStrategy GetStrategy(FightActionComponent fightActionComponent)
    {
        if (_cache.TryGetValue(fightActionComponent.Strategy, out var strategy))
            return strategy;

        return _cache[AttackStrategy.Punch];
    }
}
