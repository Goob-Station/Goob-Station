using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared.Interaction;
using Robust.Client.GameObjects;
using Robust.Client.State;
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Content.Client.Weapons.Melee;

namespace Content.Client._pofitlo.CombatExtended.AttackStrategies;

public sealed class AttackStrategyFactory
{
    private readonly IStateManager _stateManager;
    private readonly SharedInteractionSystem _interaction;
    private readonly TransformSystem _transform;
    private readonly INetManager _netManager;
    private readonly IEntityManager _entityManager;
    private readonly IPrototypeManager _prototypeManager;
    private readonly MeleeWeaponSystem _meleeWeaponSystem;

    public AttackStrategyFactory(
        IStateManager stateManager,
        SharedInteractionSystem interaction,
        TransformSystem transform,
        INetManager netManager,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        MeleeWeaponSystem meleeWeaponSystem)
    {
        _stateManager = stateManager;
        _interaction = interaction;
        _transform = transform;
        _netManager = netManager;
        _entityManager = entityManager;
        _prototypeManager = prototypeManager;
        _meleeWeaponSystem = meleeWeaponSystem;
    }

    public IAttackStrategy CreateStrategy(FightActionComponent fightActionComponent)
    {
        return fightActionComponent.Strategy switch
        {
            AttackStrategy.Punch => new PunchAttackStrategy(_stateManager, _interaction, _transform, _netManager, _entityManager, _prototypeManager, _meleeWeaponSystem),
            AttackStrategy.TailAttack => new TailAttackStrategy(_stateManager, _interaction, _transform, _netManager, _entityManager, _prototypeManager, _meleeWeaponSystem),
            _ => new PunchAttackStrategy(_stateManager, _interaction, _transform, _netManager, _entityManager, _prototypeManager, _meleeWeaponSystem)
        };
    }
}
