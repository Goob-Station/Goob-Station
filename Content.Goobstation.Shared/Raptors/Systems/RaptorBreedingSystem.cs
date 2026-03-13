using Content.Goobstation.Shared.Raptors.Components;
using Content.Goobstation.Shared.Raptors.Genetics;
using Content.Goobstation.Shared.Xenobiology.Components;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Raptors.Systems
{
    public sealed class RaptorBreedingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Attempt breeding between two raptors.
        /// </summary>
        public void TryBreed(EntityUid uidA, EntityUid uidB)
        {
            if (!_entMan.TryGetComponent(uidA, out RaptorComponent? a))
                return;

            if (!_entMan.TryGetComponent(uidB, out RaptorComponent? b))
                return;

            if (!_entMan.TryGetComponent(uidA, out GrammarComponent? genderA))
                return;

            if (!_entMan.TryGetComponent(uidB, out GrammarComponent? genderB))
                return;

            if (!CanBreed(uidA, uidB, a, b, genderA, genderB))
                return;

            EntityUid motherUid;
            EntityUid fatherUid;
            RaptorComponent mother;
            RaptorComponent father;

            if (genderA.Gender == Gender.Female)
            {
                motherUid = uidA;
                fatherUid = uidB;
                mother = a;
                father = b;
            }
            else
            {
                motherUid = uidB;
                fatherUid = uidA;
                mother = b;
                father = a;
            }

            SpawnBaby(motherUid, fatherUid, mother, father);
        }

        /// <summary>
        /// Checks if two raptors can breed.
        /// </summary>
        private bool CanBreed(
            EntityUid uidA,
            EntityUid uidB,
            RaptorComponent a,
            RaptorComponent b,
            GrammarComponent genderA,
            GrammarComponent genderB)
        {
            if (!a.BreedingMood || !b.BreedingMood)
                return false;

            if (genderA.Gender == genderB.Gender)
                return false;

            if (genderA.Gender != Gender.Male && genderA.Gender != Gender.Female)
                return false;

            if (genderB.Gender != Gender.Male && genderB.Gender != Gender.Female)
                return false;

            if (!_entMan.TryGetComponent(uidA, out MobGrowthComponent? growthA))
                return false;

            if (!_entMan.TryGetComponent(uidB, out MobGrowthComponent? growthB))
                return false;

            if (growthA.CurrentStage != "adult")
                return false;

            if (growthB.CurrentStage != "adult")
                return false;

            return true;
        }

        /// <summary>
        /// Spawn baby and assign genetics.
        /// </summary>
        private void SpawnBaby(EntityUid motherUid, EntityUid fatherUid, RaptorComponent mother, RaptorComponent father)
        {
            var coords = Transform(motherUid).Coordinates;

            var baby = _entMan.SpawnEntity("BaseBabyRaptor", coords);

            var babyComp = Comp<RaptorComponent>(baby);

            babyComp.Mother = MetaData(motherUid).EntityPrototype?.ID;
            babyComp.Father = MetaData(fatherUid).EntityPrototype?.ID;

            babyComp.Genes = GenerateGenes(mother, father);

            var grammar = EnsureComp<GrammarComponent>(baby);

            grammar.Gender = _random.Prob(0.5f)
                ? Gender.Male
                : Gender.Female;
        }

        /// <summary>
        /// Creates genetic modifiers for a baby raptor.
        /// </summary>
        private RaptorGenes GenerateGenes(RaptorComponent mother, RaptorComponent father)
        {
            var genes = new RaptorGenes();

            genes.AttackModifier =
                (mother.Genes.AttackModifier + father.Genes.AttackModifier) / 2f;

            genes.HealthModifier =
                (mother.Genes.HealthModifier + father.Genes.HealthModifier) / 2f;

            genes.GrowthModifier =
                (mother.Genes.GrowthModifier + father.Genes.GrowthModifier) / 2f;

            genes.AbilityModifier =
                (mother.Genes.AbilityModifier + father.Genes.AbilityModifier) / 2f;

            ApplyMutation(genes);
            InheritTraits(genes, mother, father);

            return genes;
        }

        /// <summary>
        /// Random mutation system.
        /// </summary>
        private void ApplyMutation(RaptorGenes genes)
        {
            const float mutationChance = 0.1f;

            if (!_random.Prob(mutationChance))
                return;

            genes.AttackModifier += _random.NextFloat(-0.1f, 0.1f);
            genes.HealthModifier += _random.NextFloat(-0.1f, 0.1f);
            genes.GrowthModifier += _random.NextFloat(-0.1f, 0.1f);
        }

        /// <summary>
        /// Trait inheritance.
        /// </summary>
        private void InheritTraits(RaptorGenes genes, RaptorComponent mother, RaptorComponent father)
        {
            foreach (var trait in mother.Genes.Traits)
            {
                if (_random.Prob(0.5f))
                    genes.Traits.Add(trait);
            }

            foreach (var trait in father.Genes.Traits)
            {
                if (_random.Prob(0.5f))
                    genes.Traits.Add(trait);
            }
        }
    }
}
