using Content.Server.Magic;
using Content.Shared.Movement.Pulling.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Magic;
using Content.Shared.Maps;
using Content.Shared.Storage.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;


// using Content.Server.Pulling;
// using Content.Shared.Pulling.Components;
// using Content.Shared._Pirate.Mage.Events;


namespace Content.Pirate.Server.Mage.EntitySystems;


public sealed class MageProgectileSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly MageManaSystem _mana = default!;
    [Dependency] private readonly SharedMapSystem _sharedMapSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    //[Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MageProjectileSpellEvent>(OnProjectileSpell);
    }

    private void OnProjectileSpell(MageProjectileSpellEvent ev)
    {
        if (!_entity.TryGetComponent<MageComponent>(ev.Performer, out var comp) || // Not a Mage
            _entity.TryGetComponent<HandcuffComponent>(ev.Performer, out var cuffs) || // handcuffed
            _entity.HasComponent<InsideEntityStorageComponent>(ev.Performer)) // Inside an entity storage
            return;

        if (ev.Handled)
            return;
        if (!_mana.TryUseAbility(ev.Performer, comp, ev.ManaCost))
            return;

        ev.Handled = true;
        //_magic.Speak(ev);

        // Take power and deal stamina damage
        // _mana.TryAddPowerLevel(comp.Owner, -ev.ManaCost);

        var xform = Transform(ev.Performer);
        var userVelocity = _physics.GetMapLinearVelocity(ev.Performer);

        foreach (var pos in GetSpawnPositions(xform, ev.Pos))
        {
            // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
            var mapPos = pos.ToMap(EntityManager, _transformSystem);
            var spawnCoords = _mapManager.TryFindGridAt(mapPos, out var gridUid, out _)
                ? pos.WithEntityId(gridUid, EntityManager)
                : new EntityCoordinates(_mapManager.GetMapEntityId(mapPos.MapId), mapPos.Position);

            var ent = Spawn(ev.Prototype, spawnCoords);
            var direction = ev.Target.ToMapPos(EntityManager, _transformSystem) -
                spawnCoords.ToMapPos(EntityManager, _transformSystem);
            _gunSystem.ShootProjectile(ent, direction, userVelocity, ev.Performer, ev.Performer);
        }
    }

    private List<EntityCoordinates> GetSpawnPositions(TransformComponent casterXform, MagicInstantSpawnData data)
    {
        switch (data)
        {
            case TargetCasterPos:
                return new List<EntityCoordinates>(1) { casterXform.Coordinates };
            case TargetInFront:
            {
                if (casterXform.GridUid == null ||
                    !TryComp<MapGridComponent>(casterXform.GridUid, out var mapGrid))
                {
                    return new List<EntityCoordinates>();
                }

                var casterPos = casterXform.Coordinates;
                if (!casterPos.TryGetTileRef(out var tileReference, EntityManager, _mapManager))
                {
                    return new List<EntityCoordinates>();
                }

                var tileIndex = tileReference.Value.GridIndices;

                var coords = _sharedMapSystem.GridTileToLocal(casterXform.GridUid.Value, mapGrid, tileIndex);
                EntityCoordinates coordsPlus;
                EntityCoordinates coordsMinus;

                var dir = casterXform.LocalRotation.GetCardinalDir();
                switch (dir)
                {
                    case Direction.North:
                    case Direction.South:
                        coordsPlus = _sharedMapSystem.GridTileToLocal(
                            casterXform.GridUid.Value,
                            mapGrid,
                            tileIndex + (1, 0));
                        coordsMinus = _sharedMapSystem.GridTileToLocal(
                            casterXform.GridUid.Value,
                            mapGrid,
                            tileIndex + (-1, 0));
                        return new List<EntityCoordinates>
                        {
                            coords,
                            coordsPlus,
                            coordsMinus
                        };

                    case Direction.East:
                    case Direction.West:
                        coordsPlus = _sharedMapSystem.GridTileToLocal(
                            casterXform.GridUid.Value,
                            mapGrid,
                            tileIndex + (0, 1));
                        coordsMinus = _sharedMapSystem.GridTileToLocal(
                            casterXform.GridUid.Value,
                            mapGrid,
                            tileIndex + (0, -1));
                        return new List<EntityCoordinates>
                        {
                            coords,
                            coordsPlus,
                            coordsMinus
                        };

                    default:
                        return new List<EntityCoordinates> { coords };
                }
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
