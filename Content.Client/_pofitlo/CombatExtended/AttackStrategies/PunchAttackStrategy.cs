using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Client.State;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.GameObjects;
using Content.Shared.Interaction;
using Robust.Shared.Network;
using Robust.Client.GameObjects;
using Content.Client.Gameplay;

namespace Content.Client._pofitlo.CombatExtended.AttackStrategies;

public sealed class PunchAttackStrategy : IAttackStrategy
{
    private readonly IStateManager _stateManager;
    private readonly SharedInteractionSystem _interaction;
    private readonly TransformSystem _transform;
    private readonly INetManager _netManager;
    private readonly IEntityManager _entityManager;

    public PunchAttackStrategy(
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

    public void ExecuteLightAttack(EntityUid attacker, MapCoordinates mousePos, EntityCoordinates coordinates, EntityUid weaponUid, MeleeWeaponComponent meleeComponent)
    {
        var attackerPos = _transform.GetMapCoordinates(attacker);

        if (mousePos.MapId != attackerPos.MapId || (attackerPos.Position - mousePos.Position).Length() > meleeComponent.Range)
            return;

        EntityUid? target = null;

        if (_stateManager.CurrentState is GameplayStateBase screen)
            target = screen.GetDamageableClickedEntity(mousePos);

        // Don't light-attack if interaction will be handling this instead
        if (_interaction.CombatModeCanHandInteract(attacker, target))
            return;

        _entityManager.RaisePredictiveEvent(new LightAttackEvent(
            _entityManager.GetNetEntity(target),
            _entityManager.GetNetEntity(weaponUid),
            _entityManager.GetNetCoordinates(coordinates)));
    }

    public void ExecuteHeavyAttack(EntityUid attacker, EntityCoordinates coordinates, EntityUid weaponUid, MeleeWeaponComponent meleeComponent)
    {
    }

    public void ExecuteDisarm(EntityUid attacker, MapCoordinates mousePos, EntityCoordinates coordinates)
    {
        EntityUid? target = null;

        if (_stateManager.CurrentState is GameplayStateBase screen)
            target = screen.GetDamageableClickedEntity(mousePos);

    }
}
