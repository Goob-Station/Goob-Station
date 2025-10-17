using Content.Goobstation.Shared.Raptors.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Raptors.Systems
{
    public sealed partial class RaptorSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;

        /// <summary>
        /// How often we check for raptor growth
        /// </summary>
        private const float GrowthCheckIntervalSeconds = 2f;

        /// <summary>
        /// Last growth check timestamp
        /// </summary>
        private TimeSpan _nextGrowthTick;

        public override void Initialize()
        {
            base.Initialize();
            _nextGrowthTick = _timing.CurTime + TimeSpan.FromSeconds(GrowthCheckIntervalSeconds);
        }

        public override void Update(float frameTime)
        {
            var curTime = _timing.CurTime;
            if (curTime < _nextGrowthTick)
                return;

            _nextGrowthTick = curTime + TimeSpan.FromSeconds(GrowthCheckIntervalSeconds);

            foreach (var (raptor, transform) in EntityQuery<RaptorComponent, TransformComponent>())
            {
                ProcessGrowth(raptor, transform);
            }
        }

        private void ProcessGrowth(RaptorComponent raptor, TransformComponent transform)
        {
            // Only baby raptors can grow
            if (raptor.GrowthPath == null)
                return;

            // Check optional conditions: happiness, feeding, proximity to parent
            var growthModifier = 1f;
            growthModifier += raptor.Happiness / 100f * 0.5f; // happiness boosts growth speed by up to 50%
            // Optionally: boost if near parent, fed recently, etc.

            raptor.GrowthProgress += 0.05f * growthModifier; // increment growth

            if (raptor.GrowthProgress < 1f)
                return;

            // Ready to grow into adult
            HatchRaptor(raptor, transform);
        }

        private void HatchRaptor(RaptorComponent baby, TransformComponent transform)
        {
            if (string.IsNullOrEmpty(baby.GrowthPath))
                return;

            // GrowthPath is now the prototype ID string of the adult raptor
            var adult = _entMan.SpawnEntity(baby.GrowthPath, transform.Coordinates);

            if (_entMan.TryGetComponent<RaptorComponent>(adult, out var adultComp))
            {
                // Apply inherited stats
                adultComp.MaxHealth += baby.MaxHealth + baby.HealthModifier;
                adultComp.CurrentHealth = adultComp.MaxHealth;
                adultComp.MeleeDamageLower += baby.MeleeDamageLower + baby.AttackModifier;
                adultComp.MeleeDamageUpper += baby.MeleeDamageUpper + baby.AttackModifier;

                // Carry over traits
                adultComp.IsMotherly = baby.IsMotherly;
                adultComp.IsPlayful = baby.IsPlayful;
                adultComp.IsCoward = baby.IsCoward;
                adultComp.IsHealer = baby.IsHealer;
            }

            _entMan.QueueDeleteEntity(baby.Owner);
        }
    }
}
