using Content.Goobstation.Common.Effects;
using Content.Server.Chat.TypingIndicator;
using Content.Server.Humanoid;
using Content.Server.Popups;
using Content.Shared._CorvaxGoob.AppearanceConverter;
using Content.Shared._CorvaxGoob.TTS;
using Content.Shared._EinsteinEngines.HeightAdjust;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.DoAfter;
using Content.Shared.Forensics.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Tools.Components;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server._CorvaxGoob.AppearanceConverter;

public sealed class AppearanceConverterSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly GrammarSystem _grammar = default!;
    [Dependency] private readonly SharedIdentitySystem _identity = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;
    [Dependency] private readonly TypingIndicatorSystem _typingIndicator = default!;
    [Dependency] private readonly HeightAdjustSystem _heightAdjust = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AppearanceConverterComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<AppearanceConverterComponent, GotUnequippedEvent>(OnGotUnequipped);

        SubscribeLocalEvent<AppearanceConverterComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<AppearanceConverterComponent, AppearanceConverterDoAfterEvent>(OnDoAfterComplete);

        SubscribeLocalEvent<AppearanceConverterComponent, AppearanceConverterDNAScanDataMessage>(OnDNAScanData);
        SubscribeLocalEvent<AppearanceConverterComponent, AppearanceConverterSelectProfileMessage>(OnSelectProfile);
        SubscribeLocalEvent<AppearanceConverterComponent, AppearanceConverterTransformMessage>(OnTransformMessage);
        SubscribeLocalEvent<AppearanceConverterComponent, AppearanceConverterDeTransformMessage>(OnDeTransformMessage);

        SubscribeLocalEvent<AppearanceConverterComponent, BoundUIOpenedEvent>(OnUiOpen);

    }

    private void OnAfterInteract(Entity<AppearanceConverterComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled
            || args.Target == null
            || !HasComp<HumanoidAppearanceComponent>(args.Target)
            || !HasComp<DnaComponent>(args.Target))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.ScanningDoAfterTime, new AppearanceConverterDoAfterEvent(), ent, target: args.Target, used: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            BreakOnDropItem = true,
            NeedHand = true,
            AttemptFrequency = AttemptFrequency.EveryTick
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfterComplete(Entity<AppearanceConverterComponent> ent, ref AppearanceConverterDoAfterEvent args)
    {
        if (args.Cancelled || args.Target is null)
            return;

        if (!TryComp<DnaComponent>(args.Target, out var dnaComp) || dnaComp.DNA is null)
            return;

        UpdateDNAProfile(ent, dnaComp.DNA);
        _popupSystem.PopupEntity(Loc.GetString("appearance-converter-scan-complete"), ent, args.User);
    }
    private void OnComponentShutdown(Entity<AppearanceConverterComponent> ent, ref ComponentShutdown args)
    {
        if (!ent.Comp.Transformed
            || ent.Comp.OriginalForm is null
            || ent.Comp.Equipee is null)
            return;

        TransformEntity(ent.Comp.Equipee.Value, ent.Comp.OriginalForm.Value, ent.Comp.TransformSound);
    }

    private void OnGotUnequipped(Entity<AppearanceConverterComponent> ent, ref GotUnequippedEvent args)
    {
        if (!ent.Comp.Transformed || ent.Comp.OriginalForm is null)
            return;

        DeTransformEntity(ent, args.Equipee);
    }

    public void DeTransformEntity(Entity<AppearanceConverterComponent> ent, EntityUid target)
    {
        if (ent.Comp.OriginalForm is null || !ent.Comp.Transformed)
            return;

        TransformEntity(target, ent.Comp.OriginalForm.Value, ent.Comp.TransformSound);

        ent.Comp.OriginalForm = null;
        ent.Comp.Transformed = false;
        ent.Comp.Equipee = null;

        UpdateUiState(ent);
    }

    private void OnUiOpen(Entity<AppearanceConverterComponent> ent, ref BoundUIOpenedEvent args) => UpdateUiState(ent);

    /// <summary>
    /// Вызывается при выборе профиля в интерфейсе.
    /// </summary>
    private void OnSelectProfile(Entity<AppearanceConverterComponent> ent, ref AppearanceConverterSelectProfileMessage args)
    {
        if (args.DNA is null || !ent.Comp.ProfilesVisualData.ContainsKey(args.DNA))
            return;

        ent.Comp.SelectedProfile = args.DNA;
    }

    /// <summary>
    /// Вызывается при вводе и сканирования ДНК в интерфейсе.
    /// </summary>
    private void OnDNAScanData(Entity<AppearanceConverterComponent> ent, ref AppearanceConverterDNAScanDataMessage args)
    {
        if (args.DNA is null || args.DNA == "")
        {
            _popupSystem.PopupEntity(Loc.GetString("appearance-converter-scan-incorrect-dna"), ent, args.Actor);
            return;
        }

        var rawDna = args.DNA.ToLower();

        foreach (var savedDnaKey in ent.Comp.ProfilesVisualData.Keys)
            if (savedDnaKey.ToLower() == rawDna)
            {
                UpdateDNAProfile(ent, savedDnaKey);
                _popupSystem.PopupEntity(Loc.GetString("appearance-converter-scan-complete"), ent, args.Actor);

                return;
            }

        var query = EntityQueryEnumerator<DnaComponent, HumanoidAppearanceComponent>();

        while (query.MoveNext(out var uid, out var dna, out var humanoidAppearance))
        {
            if (dna.DNA is not null
                && rawDna == dna.DNA.ToLower())
            {
                UpdateDNAProfile(uid, humanoidAppearance, ent);
                _popupSystem.PopupEntity(Loc.GetString("appearance-converter-scan-complete"), ent, args.Actor);

                return;
            }
        }

        _popupSystem.PopupEntity(Loc.GetString("appearance-converter-scan-incorrect-dna"), ent, args.Actor);
    }

    /// <summary>
    /// Вызывается при нажатии превращения в интерфейсе.
    /// </summary>
    private void OnTransformMessage(Entity<AppearanceConverterComponent> ent, ref AppearanceConverterTransformMessage args)
    {
        if (ent.Comp.NextTransformTime > _timing.CurTime)
            return;

        if (ent.Comp.SelectedProfile is null || ent.Comp.Transformed)
            return;

        if (!TryComp<InventoryComponent>(args.Actor, out var inventoryComponent))
            return;

        if (!_inventory.TryGetContainerSlotEnumerator((args.Actor, inventoryComponent),
            out var inventorySlotEnumerator, ent.Comp.Slots))
            return;

        bool isWeared = false;

        // Проверяем на наличия устройства в слотах
        while (inventorySlotEnumerator.NextItem(out var item))
        {
            if (item == ent.Owner)
            {
                isWeared = true;
                break;
            }
        }

        if (!isWeared)
        {
            _popupSystem.PopupEntity(Loc.GetString("appearance-converter-must-be-weared"), ent, args.Actor);
            return;
        }

        if (ent.Comp.OriginalForm is null)
            ent.Comp.OriginalForm = GenerateTransformProfile(args.Actor);

        if (ent.Comp.OriginalForm is null)
        {
            _popupSystem.PopupEntity(Loc.GetString("appearance-converter-generate-original-profile-error"), ent, args.Actor);
            return;
        }

        ent.Comp.Transformed = true;
        ent.Comp.Equipee = args.Actor;

        var profile = MergeDetailAndVisualProfile(ent.Comp.ProfilesDetailData[ent.Comp.SelectedProfile], ent.Comp.ProfilesVisualData[ent.Comp.SelectedProfile]);

        TransformEntity(args.Actor, profile, ent.Comp.TransformSound);
        ent.Comp.NextTransformTime = _timing.CurTime + ent.Comp.TransformDelay;

        UpdateUiState(ent);
    }

    private void OnDeTransformMessage(Entity<AppearanceConverterComponent> ent, ref AppearanceConverterDeTransformMessage args)
    {
        DeTransformEntity(ent, args.Actor);

        UpdateUiState(ent);
    }

    /// <summary>
    /// Добавляет или обновляет профиль сущности на основе его ДНК.
    /// </summary>
    private void UpdateDNAProfile(Entity<AppearanceConverterComponent> ent, string DNA)
    {
        var query = EntityQueryEnumerator<DnaComponent>();

        while (query.MoveNext(out var uid, out var dnaComp))
            if (DNA == dnaComp.DNA)
                UpdateDNAProfile((uid, dnaComp), ent);
    }

    /// <summary>
    /// Добавляет или обновляет профиль сущности на основе его ДНК.
    /// </summary>
    private void UpdateDNAProfile(Entity<DnaComponent> dnaEnt, Entity<AppearanceConverterComponent> converterEnt)
    {
        if (!TryComp<HumanoidAppearanceComponent>(dnaEnt, out var humanoidAppearance) || dnaEnt.Comp.DNA is null)
            return;

        UpdateDNAProfile(dnaEnt.Owner, humanoidAppearance, converterEnt);
    }

    /// <summary>
    /// Добавляет или обновляет профиль сущности на основе его ДНК.
    /// </summary>
    private void UpdateDNAProfile(EntityUid uid, HumanoidAppearanceComponent humanoidAppearance, Entity<AppearanceConverterComponent> converterEnt)
    {
        var transferProfile = GenerateTransformProfile(uid, humanoidAppearance);

        if (transferProfile is null)
            return;

        var (detail, visual) = SplitDetailAndVisualProfile(transferProfile.Value);

        converterEnt.Comp.ProfilesDetailData[transferProfile.Value.DNA] = detail;
        converterEnt.Comp.ProfilesVisualData[transferProfile.Value.DNA] = visual;
        converterEnt.Comp.SelectedProfile = transferProfile.Value.DNA;

        UpdateUiState(converterEnt);
    }

    private void UpdateUiState(Entity<AppearanceConverterComponent> entity)
    {
        var bound = new AppearanceConverterBoundUserInterfaceState();

        bound.SelectedProfile = entity.Comp.SelectedProfile;
        bound.Profiles = entity.Comp.ProfilesVisualData;
        bound.Transformed = entity.Comp.Transformed;
        bound.NextTransformTime = entity.Comp.NextTransformTime;

        Dirty(entity);
        _userInterface.SetUiState(entity.Owner, AppearanceConverterUiKey.Key, bound);
    }

    /// <summary>
    /// Комбинирует <see cref="AppearanceConverterDetailTransformProfile"/> и <see cref="AppearanceConverterVisualTransformProfile"/> для создания
    /// общего профиля <see cref="TransformProfile"/>.
    /// </summary>
    private static TransformProfile MergeDetailAndVisualProfile(AppearanceConverterDetailTransformProfile detail, AppearanceConverterVisualTransformProfile visual)
    {
        var profile = new TransformProfile();

        profile.Name = visual.Name;
        profile.Fingerprint = visual.Fingerprint;
        profile.InventorySpecies = detail.InventorySpecies;

        profile.Displacements = detail.Displacements;
        profile.MaleDisplacements = detail.MaleDisplacements;
        profile.FemaleDisplacements = detail.FemaleDisplacements;

        profile.DNA = visual.DNA;
        profile.Voice = detail.Voice;
        profile.Gender = detail.Gender;
        profile.Age = visual.Age;
        profile.Scale = detail.Scale;

        profile.SpeciesPrototype = visual.SpeciesPrototype;
        profile.SpeechVerbPrototype = detail.SpeechVerbPrototype;
        profile.TypingIndicator = detail.TypingIndicator;
        profile.SpeechSounds = detail.SpeechSounds;
        profile.EmoteSounds = detail.EmoteSounds;

        profile.SkinColor = visual.SkinColor;
        profile.EyesColor = visual.EyesColor;
        profile.Sex = visual.Sex;
        profile.Markings = visual.Markings;

        return profile;
    }

    /// <summary>
    /// Разделяет общий создаваемый профиль <see cref="TransformProfile"/> на два отдельных <see cref="AppearanceConverterDetailTransformProfile"/>
    /// и <see cref="AppearanceConverterVisualTransformProfile"/> для последующего распределения для сервера и клиента.
    /// </summary>
    private static (AppearanceConverterDetailTransformProfile detail, AppearanceConverterVisualTransformProfile visual) SplitDetailAndVisualProfile(TransformProfile profile)
    {
        var detail = new AppearanceConverterDetailTransformProfile();
        var visual = new AppearanceConverterVisualTransformProfile();

        visual.Name = profile.Name;
        visual.Fingerprint = profile.Fingerprint;
        detail.InventorySpecies = profile.InventorySpecies;

        detail.Displacements = profile.Displacements;
        detail.MaleDisplacements = profile.MaleDisplacements;
        detail.FemaleDisplacements = profile.FemaleDisplacements;

        visual.DNA = profile.DNA;
        detail.Voice = profile.Voice;
        detail.Gender = profile.Gender;
        visual.Age = profile.Age;
        detail.Scale = profile.Scale;

        visual.SpeciesPrototype = profile.SpeciesPrototype;
        detail.SpeechVerbPrototype = profile.SpeechVerbPrototype;
        detail.TypingIndicator = profile.TypingIndicator;
        detail.SpeechSounds = profile.SpeechSounds;
        detail.EmoteSounds = profile.EmoteSounds;

        visual.SkinColor = profile.SkinColor;
        visual.EyesColor = profile.EyesColor;
        visual.Sex = profile.Sex;
        visual.Markings = profile.Markings;

        return (detail, visual);
    }

    /// <summary>
    /// Полность визуально преобразовывает сущность на основе профиля.
    /// </summary>
    /// <param name="target">Цель для превращения</param>
    /// <param name="profile">Профиль в который будет преобразована сущность</param>
    /// <param name="transformSound">Звук превращения</param>
    /// <param name="doEffects">Создавать ли искры</param>
    public void TransformEntity(EntityUid target, TransformProfile profile, SoundSpecifier? transformSound = null, bool doEffects = true)
    {
        if (profile.SpeciesPrototype is null)
            return;

        var speciesPrototype = _prototype.Index<SpeciesPrototype>(profile.SpeciesPrototype);

        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoidAppearance))
            return;

        if (EnsureComp<TypingIndicatorComponent>(target, out var typingIndicator) && profile.TypingIndicator is not null)
        {
            _typingIndicator.SetTypingIndicator((target, typingIndicator), profile.TypingIndicator.Value);
            Dirty(target, typingIndicator);
        }

        humanoidAppearance.Species = speciesPrototype;

        if (profile.Name is not null)
            _meta.SetEntityName(target, profile.Name);

        if (profile.Voice is not null && TryComp<TTSComponent>(target, out var tts))
            tts.VoicePrototypeId = profile.Voice;

        _heightAdjust.SetScale(target, profile.Scale);

        if (profile.SkinColor is not null)
            humanoidAppearance.SkinColor = profile.SkinColor.Value;

        if (profile.EyesColor is not null)
            humanoidAppearance.EyeColor = profile.EyesColor.Value;

        if (profile.Age is not null)
            humanoidAppearance.Age = profile.Age.Value;

        if (profile.Sex is not null)
            _humanoidAppearance.SetSex(target, profile.Sex.Value, false, humanoidAppearance);

        if (TryComp<InventoryComponent>(target, out var inventory))
        {
            _inventory.SetSpeciesId((target, inventory), profile.InventorySpecies);
            _inventory.SetDisplacements((target, inventory), profile.Displacements, profile.MaleDisplacements, profile.FemaleDisplacements);

            if (TryComp<AppearanceComponent>(target, out var appearance))
                Dirty(target, appearance);

            Dirty(target, inventory);
        }

        if (TryComp<SpeechComponent>(target, out var speech))
        {
            if (profile.SpeechVerbPrototype is not null)
                speech.SpeechVerb = profile.SpeechVerbPrototype.Value;

            speech.SpeechSounds = profile.SpeechSounds;
        }

        if (TryComp<VocalComponent>(target, out var vocal))
        {
            vocal.EmoteSounds = profile.EmoteSounds;
            Dirty(target, vocal);
        }

        if (profile.Markings is not null)
            humanoidAppearance.MarkingSet = profile.Markings;

        if (profile.Gender is not null)
        {
            humanoidAppearance.Gender = profile.Gender.Value;

            if (TryComp<GrammarComponent>(target, out var grammar))
                _grammar.SetGender((target, grammar), profile.Gender);
        }

        if (transformSound is not null)
            _audio.PlayPvs(transformSound, target);

        if (doEffects)
            _sparks.DoSparks(Transform(target).Coordinates, playSound: false);

        _identity.QueueIdentityUpdate(target);
        Dirty(target, humanoidAppearance);
    }

    /// <summary>
    /// Создаёт экземпляр профиля типа <see cref="TransformProfile"/>, содержащий в себе всю внешнию и внутреннию информацию о указанной сущности для далнейшего преобразования.
    /// </summary>
    public TransformProfile? GenerateTransformProfile(EntityUid entityUid)
    {
        if (!TryComp<HumanoidAppearanceComponent>(entityUid, out var humanoidAppearance))
            return null;

        return GenerateTransformProfile(entityUid, humanoidAppearance);
    }

    /// <summary>
    /// Создаёт экземпляр профиля типа <see cref="TransformProfile"/>, содержащий в себе всю внешнию и внутреннию информацию о указанной сущности для далнейшего преобразования.
    /// </summary>
    public TransformProfile? GenerateTransformProfile(EntityUid entityUid, HumanoidAppearanceComponent humanoidAppearance)
    {
        if (!TryComp<DnaComponent>(entityUid, out var dna) || dna.DNA is null)
            return null;

        var profile = new TransformProfile();
        var species = _prototype.Index<SpeciesPrototype>(humanoidAppearance.Species);

        profile.DNA = dna.DNA;

        profile.Scale = new Vector2(humanoidAppearance.Width, humanoidAppearance.Height);

        profile.Name = MetaData(entityUid).EntityName;
        profile.SkinColor = humanoidAppearance.SkinColor;
        profile.SpeciesPrototype = species.ID;

        profile.EyesColor = humanoidAppearance.EyeColor;
        profile.Age = humanoidAppearance.Age;
        profile.Sex = humanoidAppearance.Sex;

        if (TryComp<FingerprintComponent>(entityUid, out var fingerprint))
            profile.Fingerprint = fingerprint.Fingerprint;

        if (TryComp<InventoryComponent>(entityUid, out var inventory))
        {
            profile.InventorySpecies = inventory.SpeciesId;

            profile.Displacements = inventory.Displacements;
            profile.MaleDisplacements = inventory.MaleDisplacements;
            profile.FemaleDisplacements = inventory.FemaleDisplacements;
        }

        if (TryComp<TypingIndicatorComponent>(entityUid, out var typingIndicator))
            profile.TypingIndicator = typingIndicator.TypingIndicatorPrototype;
        else
            profile.TypingIndicator = TypingIndicatorSystem.InitialIndicatorId;

        if (TryComp<VocalComponent>(entityUid, out var vocal))
            profile.EmoteSounds = vocal.EmoteSounds;

        if (TryComp<SpeechComponent>(entityUid, out var speech))
        {
            profile.SpeechVerbPrototype = speech.SpeechVerb;
            profile.SpeechSounds = speech.SpeechSounds;
        }

        if (TryComp<TTSComponent>(entityUid, out var tts))
            profile.Voice = tts.VoicePrototypeId;

        profile.Markings = humanoidAppearance.MarkingSet;

        return profile;
    }
}
