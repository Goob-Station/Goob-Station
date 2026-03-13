using Content.Goobstation.Shared.Raptors.Components;
using Content.Goobstation.Shared.Raptors.Genetics;
using Content.Goobstation.Shared.Xenobiology.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Raptors.Systems
{
    /// <summary>
    /// Handles continuous raptor lifecycle systems such as
    /// growth, happiness, stat updates, and trait effects.
    /// </summary>
    public sealed partial class RaptorSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MobGrowthComponent, ComponentHandleState>(OnGrowthStageChanged);
            SubscribeLocalEvent<RaptorComponent, ComponentInit>(OnRaptorInit);

        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<RaptorComponent>();

            while (query.MoveNext(out var uid, out var comp))
            {
                UpdateHappiness(uid, comp, frameTime);
            }
        }

        private void OnGrowthStageChanged(EntityUid uid, MobGrowthComponent comp, ref ComponentHandleState args)
        {
            if (!TryComp<RaptorComponent>(uid, out var raptor))
                return;

            // TO DO: More complex shit later, this is for testing purposes
            if (comp.CurrentStage == "adult")
            {
                raptor.BreedingMood = true;
                ApplyStats(uid, raptor);
            }
        }

        /// <summary>
        /// Handles happiness decay and trait effects.
        /// </summary>
        private void UpdateHappiness(EntityUid uid, RaptorComponent comp, float frameTime)
        {
            // Passive happiness decay

            // Hunger penalty

            // Trait modifiers
            // Depressed -> faster decay
            // Playful -> slower decay
        }

        /// <summary>
        /// Recalculates stats using prototype base values and genetic modifiers.
        /// </summary>
        private void ApplyStats(EntityUid uid, RaptorComponent comp)
        {
            // Get raptor prototype

            // baseStats * geneModifier

            // apply to health/melee/etc
        }

        /// <summary>
        /// Handles trait based behavior logic.
        /// </summary>
        private void ApplyTraitEffects(EntityUid uid, RaptorComponent comp)
        {
            // Coward -> flee if low health

            // Troublemaker -> attack nearby raptors

            // Motherly -> boost baby growth nearby
        }

        private void OnRaptorInit(EntityUid uid, RaptorComponent comp, ref ComponentInit args)
        {
            if (comp.Mother != null || comp.Father != null)
                return;

            if (!TryComp<MobGrowthComponent>(uid, out var growth))
                return;

            if (growth.CurrentStage != "baby")
                return;

            var breeding = EntityManager.System<RaptorBreedingSystem>();
            breeding.InitializeWildBaby(uid);
        }

    }
}
