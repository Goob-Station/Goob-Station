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

namespace Content.Pirate.Server.CentCom
{
    public sealed class MakeACentComSystem : EntitySystem
    {
        private const string ShuttlePath = "Maps/_Pirate/Shuttles/Qazmlp/shuttle_CentCom_atmos.yml";

        [ValidatePrototypeId<RandomHumanoidSettingsPrototype>]
        private const string Official = "CentcomOfficial";

        [ValidatePrototypeId<EntityPrototype>] private const string Disk = "CoordinatesDisk";
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly MapLoaderSystem _map = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly RandomHumanoidSystem _randomHumanoidSystem = default!;
        [Dependency] private readonly RandomMetadataSystem _randomMetadataSystem = default!;
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

        public void MakeAnOfficial(EntityUid entity)
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

            var centCom = _roundEndSystem.GetCentcomm();
            if (centCom == null)
                return;

            var centComComponent = _entManager.GetComponent<FTLDestinationComponent>(centCom.Value);
            centComComponent.Enabled = true;
            centComComponent.RequireCoordinateDisk = true;
            _entManager.Dirty(centCom.Value, centComComponent);

            var shuttleMap = _mapManager.CreateMap();

            var options = MapLoadOptions.Default with
            {
                DeserializationOptions = DeserializationOptions.Default with {InitializeMaps = true}
            };

            if (!_map.TryLoadGeneric(new ResPath(ShuttlePath), out var result, options))
                return;

            var shuttle = result?.Grids?.FirstOrNull();
            if (shuttle == null)
                return;

            var spawn = Transform(shuttle.Value).Coordinates;
            var uid = _randomHumanoidSystem.SpawnRandomHumanoid(Official, spawn, metadata.EntityName);
            RemComp<GhostRoleComponent>(uid);
            mindSystem.TransferTo(mind, uid, true);

            var disk = EntityManager.SpawnEntity(Disk, spawn);
            var cd = _entManager.EnsureComponent<ShuttleDestinationCoordinatesComponent>(disk);
            cd.Destination = centCom;
            _entManager.Dirty(disk, cd);
        }
    }
}
