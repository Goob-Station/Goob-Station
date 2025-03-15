using Content.Shared.Store;
using Content.Shared.Mobs.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;

namespace Content.Server._Goobstation.NTR
{
    /// <summary>
    /// Allows to buy an item only if a certain percentage of crew is dead.
    /// The percentage is customized in the prototype.
    /// </summary>
    [DataDefinition]
    public sealed partial class ListingCrewDeathCondition : ListingCondition
    {
        /// <summary>
        /// What percentage of the crew should be dead to buy this item (0.0 - 1.0).
        /// </summary>
        [DataField(required: true)]
        public float RequiredDeathPercentage { get; set; } = 0.5f; // Default: 50%

        public override bool Condition(ListingConditionArgs args)
        {
            var entityManager = args.EntityManager;
            var mobStateSystem = entityManager.System<MobStateSystem>();

            if (mobStateSystem == null)
                return false;

            // getting all the entities with mindcomponent (crew)
            int totalPlayers = 0;
            int deadPlayers = 0;

            foreach (var mind in entityManager.EntityQuery<MindComponent>())
            {
                if (mind.OwnedEntity is not { } entity)
                    continue;

                totalPlayers++;

                if (entityManager.TryGetComponent<MobStateComponent>(entity, out var mobState) &&
                    mobStateSystem.IsDead(entity))
                {
                    deadPlayers++;
                }
            }

            if (totalPlayers == 0)
                return false; // if no player, no buy

            var deathRatio = (float)deadPlayers / totalPlayers;
                return deathRatio >= RequiredDeathPercentage;
        }
    }

}
