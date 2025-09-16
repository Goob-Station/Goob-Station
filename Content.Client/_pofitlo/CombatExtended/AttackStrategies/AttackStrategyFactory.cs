using Content.Shared._Goobstation.Heretic.Components;
using Robust.Client.State;
using Robust.Shared.GameObjects;
using Content.Shared.Interaction;
using Robust.Shared.Network;
using Robust.Shared.GameObjects;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Robust.Client.GameObjects;

namespace Content.Client._pofitlo.CombatExtended.AttackStrategies;

public sealed class AttackStrategyFactory
{
    private readonly IStateManager _stateManager;
    private readonly SharedInteractionSystem _interaction;
    private readonly TransformSystem _transform;
    private readonly INetManager _netManager;
    private readonly IEntityManager _entityManager;

    public AttackStrategyFactory(
        IStateManager stateManager,
        SharedInteractionSystem interaction,
        TransformSystem transform,
        INetManager netManager,
        IEntityManager entityManager)
    {
        _stateManager = stateManager;
        _interaction = interaction;
        _transform = transform;
        _netManager = netManager;
        _entityManager = entityManager;
    }

    public IAttackStrategy CreateStrategy(FightActionComponent fightActionComponent)
    {
        return fightActionComponent.Strategy switch
        {
            AttackStrategy.Punch => new PunchAttackStrategy(_stateManager, _interaction, _transform, _netManager, _entityManager),
            AttackStrategy.TailAttack => new TailAttackStrategy(_stateManager, _interaction, _transform, _netManager, _entityManager),
            _ => new PunchAttackStrategy(_stateManager, _interaction, _transform, _netManager, _entityManager)
        };
    }
}
