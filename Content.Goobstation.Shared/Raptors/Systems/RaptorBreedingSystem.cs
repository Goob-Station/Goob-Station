using Content.Goobstation.Shared.Raptors.Components;
using Content.Goobstation.Shared.Raptors.Genetics;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Raptors.Prototypes;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Goobstation.Shared.Raptors.Systems
{
    public sealed class RaptorBreedingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private List<RaptorPrototype>? _raptorTypes;

        public override void Initialize()
        {
            base.Initialize();

            _raptorTypes = _prototypeManager.EnumeratePrototypes<RaptorPrototype>().ToList();
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

            var baby = _entMan.SpawnEntity("BabyRaptor", coords);

            var babyComp = Comp<RaptorComponent>(baby);
            var config = Comp<RaptorBreedingConfigComponent>(baby);

            babyComp.RaptorType = DetermineRaptorType(mother, father, config);

            babyComp.Mother = MetaData(motherUid).EntityPrototype?.ID;
            babyComp.Father = MetaData(fatherUid).EntityPrototype?.ID;

            babyComp.Genes = GenerateGenes(mother, father, config);

            var grammar = EnsureComp<GrammarComponent>(baby);

            grammar.Gender = _random.Prob(0.5f)
                ? Gender.Male
                : Gender.Female;

            UpdateBabySprite(baby, babyComp);
        }

        /// <summary>
        /// Creates genetic modifiers for a baby raptor.
        /// </summary>
        private RaptorGenes GenerateGenes(RaptorComponent mother, RaptorComponent father, RaptorBreedingConfigComponent config)
        {
            var genes = new RaptorGenes();

            genes.AttackModifier = MutateStat(
                (mother.Genes.AttackModifier + father.Genes.AttackModifier) / 2f,
                config);

            genes.HealthModifier = MutateStat(
                (mother.Genes.HealthModifier + father.Genes.HealthModifier) / 2f,
                config);

            genes.GrowthModifier = MutateStat(
                (mother.Genes.GrowthModifier + father.Genes.GrowthModifier) / 2f,
                config);

            genes.AbilityModifier = MutateStat(
                (mother.Genes.AbilityModifier + father.Genes.AbilityModifier) / 2f,
                config);

            InheritTraits(genes, mother, father);

            return genes;
        }

        private void UpdateBabySprite(EntityUid uid, RaptorComponent comp)
        {
            if (!TryComp<MobGrowthComponent>(uid, out var growth))
                return;

            var proto = _prototypeManager.Index(comp.RaptorType);

            foreach (var (stage, data) in growth.Stages)
            {
                data.Sprite = $"{stage}_{proto.Color}";
            }
        }

        public void InitializeWildBaby(EntityUid baby)
        {
            if (!TryComp<RaptorComponent>(baby, out var comp))
                return;

            var proto = _random.Pick(_raptorTypes!);

            comp.RaptorType = proto.ID;

            comp.Genes = new RaptorGenes
            {
                AttackModifier = _random.NextFloat(0.8f, 1.2f),
                HealthModifier = _random.NextFloat(0.8f, 1.2f),
                GrowthModifier = _random.NextFloat(0.8f, 1.2f),
                AbilityModifier = _random.NextFloat(0.8f, 1.2f)
            };

            UpdateBabySprite(baby, comp);
        }

        /// <summary>
        /// Trait inheritance.
        /// </summary>
        private void InheritTraits(RaptorGenes genes, RaptorComponent mother, RaptorComponent father)
        {
            foreach (var trait in mother.Genes.Traits)
            {
                if (_random.Prob(0.5f) && !genes.Traits.Contains(trait))
                    genes.Traits.Add(trait);
            }

            foreach (var trait in father.Genes.Traits)
            {
                if (_random.Prob(0.5f) && !genes.Traits.Contains(trait))
                    genes.Traits.Add(trait);
            }
        }

        private ProtoId<RaptorPrototype> DetermineRaptorType(RaptorComponent mother, RaptorComponent father, RaptorBreedingConfigComponent config)
        {
            var motherProto = _prototypeManager.Index(mother.RaptorType);
            var fatherProto = _prototypeManager.Index(father.RaptorType);

            // Mutation roll for color mix
            if (_random.Prob(config.ColorMutationChance))
            {
                if (motherProto.MixableWith.Contains(fatherProto.ID) && motherProto.MixResult != null)
                    return new ProtoId<RaptorPrototype>(motherProto.MixResult);

                if (fatherProto.MixableWith.Contains(motherProto.ID) && fatherProto.MixResult != null)
                    return new ProtoId<RaptorPrototype>(fatherProto.MixResult);
            }

            // Otherwise inherit one parent color
            return _random.Pick(new[] { mother.RaptorType, father.RaptorType });
        }

        private float MutateStat(float value, RaptorBreedingConfigComponent config)
        {
            if (!_random.Prob(config.StatMutationChance))
                return value;

            return value + _random.NextFloat(-config.StatMutationRange, config.StatMutationRange);
        }

    }
}
