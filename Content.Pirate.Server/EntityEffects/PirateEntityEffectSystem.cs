using Content.Pirate.Shared.Vampire.Components;
using Content.Pirate.Server.Mood;
using Content.Pirate.Server.Alchemy.Components;
using Content.Pirate.Shared.Alchemy.EntityEffects;
using Content.Pirate.Shared.Witch;
using Content.Pirate.Shared.Witch.EntityEffects;
using Content.Shared._White.Other;
using Content.Shared._White.Xenomorphs.Acid.Components;
using Content.Server.Abilities.Psionics;
using Content.Server.Body.Systems;
using Content.Server.Humanoid;
using Content.Server.Stack;
using Content.Server.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.ReactionEffects;
using Content.Shared.Chemistry.ReagentEffects;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Maps;
using Content.Shared.Mood;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stacks;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Tag;

namespace Content.Pirate.Server.EntityEffects;

/// <summary>
/// Runs Pirate reagent effects that need server logic.
/// </summary>
public sealed class PirateEntityEffectSystem : EntitySystem
{
    private static readonly Dictionary<Sex, ProtoId<EmoteSoundsPrototype>> PhilosopherStoneVulpSounds = new()
    {
        { Sex.Male, "MaleVulpkanin" },
        { Sex.Female, "FemaleVulpkanin" },
        { Sex.Unsexed, "MaleVulpkanin" },
    };

    private static readonly Color[] PhilosopherStoneFurPalette =
    [
        Color.FromHex("#E8C9A1"),
        Color.FromHex("#C68A5D"),
        Color.FromHex("#705040"),
        Color.FromHex("#D9DDE6"),
        Color.FromHex("#B88A67"),
        Color.FromHex("#F2F2F2"),
    ];

    private static readonly string[] PhilosopherStoneEarMarkings =
    [
        "VulpEar",
        "VulpEarFox",
        "VulpEarWolf",
        "VulpEarFennec",
        "VulpEarCoyote",
    ];

    private static readonly string[] PhilosopherStoneTailMarkings =
    [
        "VulpTail",
        "VulpTailFox",
        "VulpTailFox2",
        "VulpTailFox3",
        "VulpTailFluffy",
    ];

    private static readonly Dictionary<string, PhilosopherStoneStackTransmutation> PhilosopherStoneTransmutations = new()
    {
        { "Steel", new("SheetGlass", 1) },
        { "Brass", new("SheetRGlass", 2) },
        { "Plasteel", new("SheetPGlass", 2) },
        { "Glass", new("MaterialDiamond", 5) },
        { "ReinforcedGlass", new("SheetPGlass", 2) },
        { "Plasma", new("IngotGold", 3) },
        { "PlasmaGlass", new("SheetUGlass", 2) },
        { "ReinforcedPlasmaGlass", new("SheetRUGlass", 2) },
        { "Uranium", new("IngotSilver", 2) },
        { "Silver", new("IngotGold", 2) },
        { "Gold", new("MaterialDiamond", 2) },
        { "MetalRod", new("SheetBrass", 2) },
        { "Cloth", new("MaterialDurathread", 2) },
        { "WoodPlank", new("MaterialCloth", 2) },
        { "Paper", new("SpaceCash", 1, 10) },
        { "Plastic", new("SpaceCashCounterfeit", 1, 10) },
        { "Cardboard", new("SheetPaper", 2) },
        { "Bananium", new("IngotGold", 2) },
        { "GoldOre", new("SilverOre1", 2) },
        { "SilverOre", new("GoldOre1", 2) },
        { "PlasmaOre", new("GoldOre1", 2) },
        { "UraniumOre", new("SilverOre1", 2) },
        { "DiamondOre", new("MaterialDiamond", 1) },
    };

    private static readonly Dictionary<string, string> PhilosopherStoneWallTransmutations = new()
    {
        { "WallSolid", "WallSilver" },
        { "WallSilver", "WallGold" },
        { "WallGold", "WallUranium" },
        { "WallWood", "WallSolid" },
        { "WallReinforced", "WallPlastitanium" },
    };

    private static readonly Dictionary<string, string> PhilosopherStoneFloorTransmutations = new()
    {
        { "Plating", "FloorDarkMarble" },
        { "FloorSteel", "FloorGold" },
        { "FloorWhite", "FloorGlass" },
        { "FloorWood", "FloorDark" },
        { "FloorDark", "FloorRGlass" },
        { "FloorTechMaint", "FloorClown" },
        { "FloorTechMaintDark", "FloorDarkMarble" },
        { "FloorMono", "FloorRGlass" },
    };

    private static readonly string[] MagicalReagents =
    [
        "WitchLove",
        "WitchParanoia",
        "WitchRage",
        "WitchInvisibility",
        "WitchMindFog",
        "WitchMirrorCurse",
        "WitchBloodBond",
        "WitchSoulShadow",
        "WitchSubstitution",
        "WitchBlackBile",
        "WitchEcho",
        "WitchNameCurse",
        "WitchHungerPotion",
        "InteractionChaosMixture",
    ];

    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilities = default!;
    [Dependency] private readonly PsionicsSystem _psionics = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly TileSystem _tileSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChangeGlimmerReactionEffect>>(OnExecuteChangeGlimmer);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRemovePsionic>>(OnExecuteChemRemovePsionic);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemAddMoodlet>>(OnExecuteChemAddMoodlet);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRemoveMoodlet>>(OnExecuteChemRemoveMoodlet);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemPurgeMoodlets>>(OnExecuteChemPurgeMoodlets);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRerollPsionic>>(OnExecuteChemRerollPsionic);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRestorePsionicReroll>>(OnExecuteChemRestorePsionicReroll);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<CleanseWitchEffects>>(OnExecuteCleanseWitchEffects);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<PirateAcidCorrode>>(OnExecuteAcidCorrode);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<PhilosopherStoneEffect>>(OnExecutePhilosopherStone);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChaosMixtureEffect>>(OnExecuteChaosMixture);
    }

    private void OnExecuteChangeGlimmer(ref ExecuteEntityEffectEvent<ChangeGlimmerReactionEffect> args)
    {
        // Only reagent reactions matter here.
        if (args.Args is not EntityEffectReagentArgs)
            return;

        _glimmer.DeltaGlimmerInput(args.Effect.Count);
    }

    private void OnExecuteChemRemovePsionic(ref ExecuteEntityEffectEvent<ChemRemovePsionic> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        if (reagentArgs.Scale != 1f)
            return;

        _psionicAbilities.MindBreak(reagentArgs.TargetEntity);
    }

    private void OnExecuteChemAddMoodlet(ref ExecuteEntityEffectEvent<ChemAddMoodlet> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        RaiseLocalEvent(reagentArgs.TargetEntity, new MoodEffectEvent(args.Effect.MoodPrototype));
    }

    private void OnExecuteChemRemoveMoodlet(ref ExecuteEntityEffectEvent<ChemRemoveMoodlet> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        RaiseLocalEvent(reagentArgs.TargetEntity, new MoodRemoveEffectEvent(args.Effect.MoodPrototype));
    }

    private void OnExecuteChemPurgeMoodlets(ref ExecuteEntityEffectEvent<ChemPurgeMoodlets> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs
            || !TryComp<MoodComponent>(reagentArgs.TargetEntity, out var moodComponent))
            return;

        var moodletList = new List<string>();

        foreach (var moodlet in moodComponent.CategorisedEffects.Values)
        {
            if (!_prototype.TryIndex(moodlet, out MoodEffectPrototype? moodProto)
                || moodProto.Timeout == 0 && !args.Effect.RemovePermanentMoodlets)
                continue;

            moodletList.Add(moodlet);
        }

        foreach (var moodlet in moodComponent.UncategorisedEffects)
        {
            if (!_prototype.TryIndex(moodlet.Key, out MoodEffectPrototype? moodProto)
                || moodProto.Timeout == 0 && !args.Effect.RemovePermanentMoodlets)
                continue;

            moodletList.Add(moodlet.Key);
        }

        foreach (var moodId in moodletList)
            RaiseLocalEvent(reagentArgs.TargetEntity, new MoodRemoveEffectEvent(moodId));
    }

    private void OnExecuteChemRerollPsionic(ref ExecuteEntityEffectEvent<ChemRerollPsionic> args)
    {
        if (args.Args is not EntityEffectReagentArgs)
            return;

        _psionics.RerollPsionics(args.Args.TargetEntity, bonusMuliplier: args.Effect.BonusMuliplier);
    }

    private void OnExecuteChemRestorePsionicReroll(ref ExecuteEntityEffectEvent<ChemRestorePsionicReroll> args)
    {
        if (args.Args is not EntityEffectReagentArgs)
            return;

        if (!TryComp(args.Args.TargetEntity, out PsionicComponent? psionicComp))
            return;

        if (!psionicComp.Roller && !args.Effect.BypassRoller)
            return;

        psionicComp.CanReroll = true;
        Dirty(args.Args.TargetEntity, psionicComp);
    }

    private void OnExecuteCleanseWitchEffects(ref ExecuteEntityEffectEvent<CleanseWitchEffects> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        foreach (var effect in WitchStatusEffectIds.WitchEffects)
        {
            _statusEffects.TryRemoveStatusEffect(reagentArgs.TargetEntity, effect);
        }

        if (!TryComp<BloodstreamComponent>(reagentArgs.TargetEntity, out var bloodstream)
            || !_solutions.ResolveSolution(reagentArgs.TargetEntity, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution, out var chemSolution))
            return;

        foreach (var magicalReagent in MagicalReagents)
        {
            var reagentId = new ReagentId(magicalReagent, null);
            var quantity = chemSolution.GetReagentQuantity(reagentId);
            if (quantity > FixedPoint2.Zero)
                _solutions.RemoveReagent(bloodstream.ChemicalSolution.Value, reagentId, quantity);
        }
    }

    private void OnExecuteAcidCorrode(ref ExecuteEntityEffectEvent<PirateAcidCorrode> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        TryApplyDirectAcid(reagentArgs.TargetEntity, (args.Effect.CausticDamage * reagentArgs.Scale).Float());
    }

    private void OnExecutePhilosopherStone(ref ExecuteEntityEffectEvent<PhilosopherStoneEffect> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        if (TryApplyDirectPhilosopherStone(reagentArgs.TargetEntity))
            return;

        TryApplyPhilosopherStoneSpeciesShift(reagentArgs.TargetEntity, reagentArgs.Quantity.Float());
    }

    public bool TryApplyDirectAcid(EntityUid target, float amount)
    {
        if (HasComp<StructureComponent>(target))
        {
            if (TryComp<AcidCorrodingComponent>(target, out var existing))
            {
                existing.AcidExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(8);
                existing.DamagePerSecond = CreateStructureAcidDamage(amount);
                Dirty(target, existing);
                return true;
            }

            var acid = SpawnAttachedTo("XenomorphAcid", Transform(target).Coordinates);
            var corroding = new AcidCorrodingComponent
            {
                Acid = acid,
                AcidExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(8),
                DamagePerSecond = CreateStructureAcidDamage(amount)
            };

            AddComp(target, corroding);

            return true;
        }

        if (!TryComp(target, out DamageableComponent? damageable))
            return false;

        var damage = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Caustic"), FixedPoint2.New(amount));
        _damageable.TryChangeDamage(target, damage, true, origin: target, ignoreBlockers: true, damageable: damageable);
        return true;
    }

    public bool TryApplyDirectPhilosopherStone(EntityUid target)
    {
        if (!TryComp(target, out StackComponent? stack))
            return false;

        if (!PhilosopherStoneTransmutations.TryGetValue(stack.StackTypeId, out var transmutation))
            return false;

        var outputCount = (stack.Count / transmutation.InputRatio) * transmutation.OutputMultiplier;
        if (outputCount <= 0)
            return TryApplyPhilosopherStoneWallTransmutation(target);

        var batches = outputCount / transmutation.OutputMultiplier;
        var consumedCount = batches * transmutation.InputRatio;
        var remainingCount = stack.Count - consumedCount;

        var coords = Transform(target).Coordinates;
        var replacement = Spawn(transmutation.OutputPrototype, coords);
        if (TryComp(replacement, out StackComponent? replacementStack))
            _stack.SetCount(replacement, outputCount, replacementStack);

        if (remainingCount > 0)
            _stack.SetCount(target, remainingCount, stack);
        else
            QueueDel(target);

        return true;
    }

    public bool TryApplyPhilosopherStoneTileTransmutation(TileRef tile)
    {
        var currentTile = _turf.GetContentTileDefinition(tile).ID;
        if (!PhilosopherStoneFloorTransmutations.TryGetValue(currentTile, out var replacementId))
            return false;

        var replacement = _prototype.Index<ContentTileDefinition>(replacementId);
        return _tileSystem.ReplaceTile(tile, replacement);
    }

    private bool TryApplyPhilosopherStoneWallTransmutation(EntityUid target)
    {
        var prototypeId = MetaData(target).EntityPrototype?.ID;
        if (prototypeId == null || !PhilosopherStoneWallTransmutations.TryGetValue(prototypeId, out var replacementId))
            return false;

        var xform = Transform(target);
        var replacement = Spawn(replacementId, xform.Coordinates);
        Transform(replacement).LocalRotation = xform.LocalRotation;
        QueueDel(target);
        return true;
    }

    private bool TryApplyPhilosopherStoneSpeciesShift(EntityUid target, float metabolizedQuantity)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid)
            || !TryComp<BloodstreamComponent>(target, out var bloodstream)
            || !_solutions.ResolveSolution(target, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution, out var chemSolution))
            return false;

        var reagentId = new ReagentId("AlchemistPhilosopherStone", null);
        var quantity = chemSolution.GetReagentQuantity(reagentId);
        var memory = EnsureComp<PhilosopherStoneSpeciesMemoryComponent>(target);

        var quantityFloat = quantity.Float();
        if (quantityFloat < 1f)
        {
            memory.TriggerReady = true;
            memory.AccumulatedIntake = 0f;
        }

        memory.LastObservedQuantity = quantityFloat;
        if (quantityFloat > 0f && memory.TriggerReady)
            memory.AccumulatedIntake += metabolizedQuantity;

        if (!memory.TriggerReady || memory.AccumulatedIntake < 15f)
            return false;

        memory.TriggerReady = false;
        memory.AccumulatedIntake = 0f;
        var currentSpecies = humanoid.Species;

        if (currentSpecies == "Vulpkanin")
        {
            return RestoreOriginalSpeciesAppearance(target, memory, humanoid);
        }

        StoreOriginalSpeciesAppearance(target, memory, humanoid);

        _humanoid.SetSpecies(target, "Vulpkanin", false, humanoid);
        humanoid.CustomBaseLayers.Clear();
        humanoid.MarkingSet.RemoveCategory(MarkingCategories.HeadTop);
        humanoid.MarkingSet.RemoveCategory(MarkingCategories.Tail);

        var furColor = _random.Pick(PhilosopherStoneFurPalette);
        _humanoid.SetSkinColor(target, furColor, false, false, humanoid);
        _humanoid.AddMarking(target, _random.Pick(PhilosopherStoneEarMarkings), furColor, false, forced: true, humanoid);
        _humanoid.AddMarking(target, _random.Pick(PhilosopherStoneTailMarkings), furColor, false, forced: true, humanoid);
        ApplyVulpSpeech(target, humanoid.Sex, humanoid);
        Dirty(target, humanoid);
        return true;
    }

    private void StoreOriginalSpeciesAppearance(EntityUid target,
        PhilosopherStoneSpeciesMemoryComponent memory,
        HumanoidAppearanceComponent humanoid)
    {
        if (memory.HasOriginalAppearance)
            return;

        memory.OriginalSpecies = humanoid.Species;
        memory.OriginalSkinColor = humanoid.SkinColor;
        memory.HasOriginalSkinColor = true;
        memory.OriginalEyeColor = humanoid.EyeColor;
        memory.OriginalSex = humanoid.Sex;
        memory.OriginalGender = humanoid.Gender;
        memory.OriginalAge = humanoid.Age;
        memory.OriginalHeight = humanoid.Height;
        memory.OriginalWidth = humanoid.Width;
        memory.OriginalBarkVoice = humanoid.BarkVoice;
        memory.OriginalMarkings = new MarkingSet(humanoid.MarkingSet);
        memory.OriginalCustomBaseLayers = new Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo>(humanoid.CustomBaseLayers);
        if (TryComp<SpeechComponent>(target, out var speech))
        {
            memory.OriginalSpeechSounds = speech.SpeechSounds;
            memory.OriginalSpeechVerb = speech.SpeechVerb;
        }

        if (TryComp<VocalComponent>(target, out var vocal))
        {
            memory.OriginalVocalSounds = vocal.Sounds != null
                ? new Dictionary<Sex, ProtoId<EmoteSoundsPrototype>>(vocal.Sounds)
                : null;
            memory.OriginalEmoteSounds = vocal.EmoteSounds;
        }

        memory.HadVulpEmotesTag = HasComp<TagComponent>(target) && _tags.HasTag(target, "VulpEmotes");
        memory.HasOriginalAppearance = true;
    }

    private bool RestoreOriginalSpeciesAppearance(EntityUid target,
        PhilosopherStoneSpeciesMemoryComponent memory,
        HumanoidAppearanceComponent humanoid)
    {
        var targetSpecies = memory.OriginalSpecies ?? "Human";
        _humanoid.SetSpecies(target, targetSpecies, false, humanoid);

        if (memory.HasOriginalSkinColor)
            _humanoid.SetSkinColor(target, memory.OriginalSkinColor, false, false, humanoid);

        _humanoid.SetSex(target, memory.OriginalSex, false, humanoid);
        _humanoid.SetGender(target, memory.OriginalGender, false, humanoid);
        humanoid.EyeColor = memory.OriginalEyeColor;
        humanoid.Age = memory.OriginalAge;
        humanoid.Height = memory.OriginalHeight;
        humanoid.Width = memory.OriginalWidth;

        if (memory.OriginalMarkings != null)
            humanoid.MarkingSet = new MarkingSet(memory.OriginalMarkings);

        humanoid.CustomBaseLayers = memory.OriginalCustomBaseLayers != null
            ? new Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo>(memory.OriginalCustomBaseLayers)
            : new Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo>();

        if (memory.OriginalBarkVoice != null)
            _humanoid.SetBarkVoice(target, memory.OriginalBarkVoice, humanoid);

        RestoreOriginalSpeech(target, memory, humanoid.Sex);
        Dirty(target, humanoid);
        return true;
    }

    private DamageSpecifier CreateStructureAcidDamage(float amount)
    {
        var damage = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Caustic"), FixedPoint2.New(amount * 1.5f));
        damage.DamageDict[_prototype.Index<DamageTypePrototype>("Heat").ID] = FixedPoint2.New(amount);
        return damage;
    }

    private void ApplyVulpSpeech(EntityUid target, Sex sex, HumanoidAppearanceComponent humanoid)
    {
        _humanoid.SetBarkVoice(target, "Vulpkanin", humanoid);

        if (TryComp<SpeechComponent>(target, out var speech))
        {
            speech.SpeechSounds = "Vulpkanin";
            speech.SpeechVerb = "Vulpkanin";
            Dirty(target, speech);
        }

        if (TryComp<VocalComponent>(target, out var vocal))
        {
            vocal.Sounds = new Dictionary<Sex, ProtoId<EmoteSoundsPrototype>>(PhilosopherStoneVulpSounds);
            vocal.EmoteSounds = PhilosopherStoneVulpSounds.GetValueOrDefault(sex, PhilosopherStoneVulpSounds[Sex.Unsexed]);
            Dirty(target, vocal);
        }

        _tags.AddTag(target, "VulpEmotes");
    }

    private void RestoreOriginalSpeech(EntityUid target, PhilosopherStoneSpeciesMemoryComponent memory, Sex sex)
    {
        if (TryComp<SpeechComponent>(target, out var speech))
        {
            speech.SpeechSounds = memory.OriginalSpeechSounds;
            speech.SpeechVerb = memory.OriginalSpeechVerb ?? "Default";
            Dirty(target, speech);
        }

        if (TryComp<VocalComponent>(target, out var vocal))
        {
            vocal.Sounds = memory.OriginalVocalSounds != null
                ? new Dictionary<Sex, ProtoId<EmoteSoundsPrototype>>(memory.OriginalVocalSounds)
                : null;
            vocal.EmoteSounds = memory.OriginalEmoteSounds;

            if (vocal.Sounds != null && vocal.Sounds.TryGetValue(sex, out var sound))
                vocal.EmoteSounds = sound;

            Dirty(target, vocal);
        }

        if (memory.HadVulpEmotesTag)
            _tags.AddTag(target, "VulpEmotes");
        else
            _tags.RemoveTag(target, "VulpEmotes");
    }

    private void OnExecuteChaosMixture(ref ExecuteEntityEffectEvent<ChaosMixtureEffect> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs || args.Effect.Reagents.Count == 0)
            return;

        if (!TryComp<BloodstreamComponent>(reagentArgs.TargetEntity, out var bloodstream))
            return;

        var reagent = _random.Pick(args.Effect.Reagents);
        var solution = new Solution(reagent, FixedPoint2.New(args.Effect.Quantity));
        _bloodstream.TryAddToChemicals((reagentArgs.TargetEntity, bloodstream), solution);
    }

    private readonly record struct PhilosopherStoneStackTransmutation(string OutputPrototype, int InputRatio, int OutputMultiplier = 1);
}
