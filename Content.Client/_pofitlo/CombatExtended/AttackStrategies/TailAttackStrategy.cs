using Content.Client.Gameplay;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Content.Shared.Interaction;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Melee.Events;
using Robust.Client.GameObjects;
using Robust.Client.GameObjects;
using Robust.Client.State;
using Robust.Client.State;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Content.Client.Weapons.Melee;

using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Client._pofitlo.CombatExtended.AttackStrategies;
using Robust.Shared.Prototypes;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Content.Client.Weapons.Melee;

namespace Content.Client._pofitlo.CombatExtended.AttackStrategies;

public sealed class TailAttackStrategy : IAttackStrategy
{
    private readonly IStateManager _stateManager;
    private readonly SharedInteractionSystem _interaction;
    private readonly TransformSystem _transform;
    private readonly INetManager _netManager;
    private readonly IEntityManager _entityManager;
    private readonly IPrototypeManager _prototypeManager;
    private readonly MeleeWeaponSystem _meleeWeaponSystem;


    public TailAttackStrategy(
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
        _prototypeManager = prototypeManager; // TODO убрать
        _meleeWeaponSystem = meleeWeaponSystem;
    }

    public void ExecuteMainAttack(EntityUid attacker, MapCoordinates mousePos, EntityCoordinates coordinates, EntityUid weaponUid, MeleeWeaponComponent meleeComponent)
    {

        var range = 2f;

        var target = _meleeWeaponSystem.GetTargetAsEntityUid(attacker, range);

        // TODO решить куда это выносить
        if (!_entityManager.TryGetComponent<FightActionComponent>(attacker, out var fightActionComp))
            return;

        if (!_prototypeManager.TryIndex(fightActionComp.CombatAnimationPrototype, out CombatAnimationPrototype? animPrototype) && animPrototype == null)
            return;

        var angle = Angle.FromDegrees(animPrototype.AngleEnd - animPrototype.AngleStart);

        var entities = _meleeWeaponSystem.GetListOfNetEntitiesInArea(attacker, coordinates, 1f, angle); // TODO настройки вынести в прототип

        // Don't light-attack if interaction will be handling this instead // TODO разобраться что это делает
        if (_interaction.CombatModeCanHandInteract(attacker, target))
            return;

        _entityManager.RaisePredictiveEvent(new TailMainAttackEvent(
            _entityManager.GetNetEntity(weaponUid),
            entities, // TODO разобраться зачем используется рейнж
            _entityManager.GetNetCoordinates(coordinates)
            ));
    } // TODO упростить

    public void ExecuteAltAttack(EntityUid attacker, EntityCoordinates coordinates, EntityUid weaponUid, MeleeWeaponComponent meleeComponent)
    {
        var target = _meleeWeaponSystem.GetTargetAsEntityUid(attacker, 2f);

        _entityManager.RaisePredictiveEvent(new TailAltAttackEvent(
        _entityManager.GetNetEntity(weaponUid),
        _entityManager.GetNetEntity(target),
        _entityManager.GetNetCoordinates(coordinates)
        ));
    }

    public void ExecuteDisarm(EntityUid attacker, MapCoordinates mousePos, EntityCoordinates coordinates)
    {
    }
}
