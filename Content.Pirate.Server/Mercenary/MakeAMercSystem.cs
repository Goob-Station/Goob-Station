using Content.Server.Ghost.Roles.Components;
using Content.Server.Humanoid.Systems;
using Content.Server.RandomMetadata;
using Content.Server.RoundEnd;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Players;
using Content.Shared.Shuttles.Components;
using Robust.Server.Player;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Pirate.Server.Mercenary
{
    public sealed class MakeAMercSystem : EntitySystem
    {
        private const string MapPath = "Maps/_Pirate/Shuttles/Qazmlp/st_merc.yml";

        [ValidatePrototypeId<RandomHumanoidSettingsPrototype>]
        private const string SpawnerPrototypeId = "Mercenary";

        [ValidatePrototypeId<EntityPrototype>] private const string Disk = "CoordinatesDisk";
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly MapLoaderSystem _map = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly RandomHumanoidSystem _randomHumanoidSystem = default!;
        [Dependency] private readonly RandomMetadataSystem _randomMetadataSystem = default!;
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

        public void MakeAnMerc(EntityUid entity)
        {
            _playerManager.TryGetSessionByEntity(entity, out var session);

            if (session is null)
                return;

            var playerCData = session.ContentData();
            if (playerCData == null)
                return;

            var mindSystem = _entManager.System<SharedMindSystem>();
            var metadata = _entManager.GetComponent<MetaDataComponent>(entity);
            var mind = playerCData.Mind ?? mindSystem.CreateMind(session.UserId, metadata.EntityName);

            var shuttleMap = _mapManager.CreateMap();

            var options = MapLoadOptions.Default with
            {
                DeserializationOptions = DeserializationOptions.Default with {InitializeMaps = true}
            };

            if (!_map.TryLoadGeneric(new ResPath(MapPath), out var result, options))
                return;

            var shuttleMapId = _mapManager.GetMapEntityId(shuttleMap);
            var shuttleMapComponent = EnsureComp<FTLDestinationComponent>(shuttleMapId);
            shuttleMapComponent.Enabled = true;
            shuttleMapComponent.RequireCoordinateDisk = true;
            _entManager.Dirty(shuttleMapId, shuttleMapComponent);

            var shuttle = result?.Grids?.FirstOrNull();
            if (shuttle == null)
                return;

            var position = Transform(shuttle.Value).Coordinates;
            var spawn = new EntityCoordinates(shuttle.Value, position.X + 10, position.Y);
            var uid = _randomHumanoidSystem.SpawnRandomHumanoid(SpawnerPrototypeId, spawn, metadata.EntityName);
            RemComp<GhostRoleComponent>(uid);
            mindSystem.TransferTo(mind, uid, true);

            var disk = EntityManager.SpawnEntity(Disk, spawn);
            var cd = _entManager.EnsureComponent<ShuttleDestinationCoordinatesComponent>(disk);
            cd.Destination = shuttleMapId;
            _entManager.Dirty(disk, cd);
        }
    }
}
