using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;
using Robust.Shared.Map.Components;

namespace Content.Server.Atmos.Reactions
{
    [UsedImplicitly, DataDefinition]
    public sealed partial class MetalHydrogenReaction : IGasReactionEffect
    {
        public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
        {
            if (holder is not TileAtmosphere tile)
                return ReactionResult.NoReaction;

            var initBZ = mixture.GetMoles(Gas.BZ);
            var initHydrogen = mixture.GetMoles(Gas.Hydrogen);

            if (initHydrogen < 300f || initBZ < 50f)
                return ReactionResult.NoReaction;

            var pressure = mixture.Pressure;
            var pressureEfficiency = Math.Min(pressure / 20000f, 1f);
            var temperature = mixture.Temperature;
            var temperatureEfficiency = Math.Min(23.2f / temperature, 1f);
            var rate = pressureEfficiency * temperatureEfficiency * 0.10f;
            var roll = (float) new Random().NextDouble();

            if (pressure < 10000f || temperature > 273.2f || roll > rate)
                return ReactionResult.NoReaction;

            mixture.AdjustMoles(Gas.Hydrogen, -300f);
            mixture.AdjustMoles(Gas.BZ, -50f);

            var tileRef = atmosphereSystem.GetTileRef(tile);

            var gridId = tileRef.GridUid;

            var entityManager = IoCManager.Resolve<IEntityManager>();
            var map = entityManager.System<SharedMapSystem>();

            if (!entityManager.TryGetComponent<MapGridComponent>(gridId, out var mapGrid))
                return ReactionResult.NoReaction;

            var coords = map.GridTileToLocal(gridId, mapGrid, tileRef.GridIndices);
            entityManager.SpawnEntity("MetalHydrogen1", coords);

            return ReactionResult.Reacting;
        }
    }
}
