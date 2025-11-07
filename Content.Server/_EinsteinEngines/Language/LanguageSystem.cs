// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Knowledge.Components;
using Content.Goobstation.Common.Knowledge.Systems;
using Content.Server.Silicons.Borgs;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language.Components;
using Content.Shared._EinsteinEngines.Language.Events;
using Content.Shared._EinsteinEngines.Language.Systems;
using Content.Shared.Body.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server._EinsteinEngines.Language;

public sealed partial class LanguageSystem : SharedLanguageSystem
{
    [Dependency] private readonly KnowledgeSystem _knowledge = default!; // Goobstation edit

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguageSpeakerComponent, MapInitEvent>(OnInitLanguageSpeaker, after: [typeof(SharedBodySystem), typeof(BorgSystem)]);
        SubscribeLocalEvent<LanguageSpeakerComponent, ComponentGetState>(OnGetLanguageState);
        SubscribeLocalEvent<UniversalLanguageSpeakerComponent, DetermineEntityLanguagesEvent>(OnDetermineUniversalLanguages);
        SubscribeNetworkEvent<LanguagesSetMessage>(OnClientSetLanguage);

        SubscribeLocalEvent<UniversalLanguageSpeakerComponent, MapInitEvent>((uid, _, _) => UpdateEntityLanguages(uid), after: [typeof(SharedBodySystem), typeof(BorgSystem)]);
        SubscribeLocalEvent<UniversalLanguageSpeakerComponent, ComponentRemove>((uid, _, _) => UpdateEntityLanguages(uid));
    }

    #region event handling

    private void OnInitLanguageSpeaker(Entity<LanguageSpeakerComponent> ent, ref MapInitEvent args)
    {
        if (string.IsNullOrEmpty(ent.Comp.CurrentLanguage))
            ent.Comp.CurrentLanguage = ent.Comp.SpokenLanguages.FirstOrDefault(UniversalPrototype);

        // Goobstation edit start
        if (TryComp(ent.Owner, out LanguageGrantComponent? grant))
        {
            if (!_knowledge.TryEnsureKnowledgeUnit(ent.Owner, LanguageKnowledgeId, out var knowledgeEnt))
            {
                Log.Error($"Entity {ToPrettyString(ent.Owner)} failed to setup {nameof(KnowledgeContainerComponent)} properly!");
                return;
            }

            var knowledge = EnsureComp<LanguageKnowledgeComponent>(knowledgeEnt.Value);

            foreach (var spoken in grant.SpokenLanguages)
                knowledge.SpokenLanguages.Add(spoken);

            foreach (var understood in grant.UnderstoodLanguages)
                knowledge.UnderstoodLanguages.Add(understood);
        }
        // Goobstation edit end

        UpdateEntityLanguages(ent!);
    }

    private void OnGetLanguageState(Entity<LanguageSpeakerComponent> entity, ref ComponentGetState args)
    {
        args.State = new LanguageSpeakerComponent.State
        {
            CurrentLanguage = entity.Comp.CurrentLanguage,
            SpokenLanguages = entity.Comp.SpokenLanguages,
            UnderstoodLanguages = entity.Comp.UnderstoodLanguages
        };
    }

    private void OnDetermineUniversalLanguages(Entity<UniversalLanguageSpeakerComponent> entity, ref DetermineEntityLanguagesEvent ev)
    {
        // We only add it as a spoken language: CanUnderstand checks for ULSC itself.
        if (entity.Comp.Enabled)
            ev.SpokenLanguages.Add(PsychomanticPrototype);
    }


    private void OnClientSetLanguage(LanguagesSetMessage message, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { Valid: true } uid)
            return;

        var language = GetLanguagePrototype(message.CurrentLanguage);
        if (language == null || !CanSpeak(uid, language.ID))
            return;

        SetLanguage(uid, language.ID);
    }

    #endregion

    #region public api

    //public bool CanUnderstand(Entity<LanguageSpeakerComponent?> ent, ProtoId<LanguagePrototype> language) // - Goob : moved to Shared
    //public bool CanSpeak(Entity<LanguageSpeakerComponent?> ent, ProtoId<LanguagePrototype> language) // - Goob : moved to Shared

    /// <summary>
    ///     Returns the current language of the given entity, assumes Universal if it's not a language speaker.
    /// </summary>
    public LanguagePrototype GetLanguage(Entity<LanguageSpeakerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, logMissing: false)
            || string.IsNullOrEmpty(ent.Comp.CurrentLanguage)
            || !_prototype.TryIndex<LanguagePrototype>(ent.Comp.CurrentLanguage, out var proto)
        )
            return Universal;

        return proto;
    }

    /// <summary>
    ///     Returns the list of languages this entity can speak.
    /// </summary>
    /// <remarks>This simply returns the value of <see cref="LanguageSpeakerComponent.SpokenLanguages"/>.</remarks>
    public List<ProtoId<LanguagePrototype>> GetSpokenLanguages(EntityUid uid)
    {
        return TryComp<LanguageSpeakerComponent>(uid, out var component) ? component.SpokenLanguages : [];
    }

    /// <summary>
    ///     Returns the list of languages this entity can understand.
    /// </summary
    /// <remarks>This simply returns the value of <see cref="LanguageSpeakerComponent.SpokenLanguages"/>.</remarks>
    public List<ProtoId<LanguagePrototype>> GetUnderstoodLanguages(EntityUid uid)
    {
        return TryComp<LanguageSpeakerComponent>(uid, out var component) ? component.UnderstoodLanguages : [];
    }

    public void SetLanguage(Entity<LanguageSpeakerComponent?> ent, ProtoId<LanguagePrototype> language)
    {
        if (!CanSpeak(ent, language)
            || !Resolve(ent, ref ent.Comp)
            || ent.Comp.CurrentLanguage == language)
            return;

        ent.Comp.CurrentLanguage = language;
        RaiseLocalEvent(ent, new LanguagesUpdateEvent(), true);
        Dirty(ent);
    }

    /// <summary>
    ///     Adds a new language to the respective lists of intrinsically known languages of the given entity.
    /// </summary>
    public void AddLanguage(
        EntityUid uid,
        ProtoId<LanguagePrototype> language,
        bool addSpoken = true,
        bool addUnderstood = true)
    {
        // Goobstation edit start
        if (!_knowledge.TryEnsureKnowledgeUnit(uid, LanguageKnowledgeId, out var knowledgeEnt))
            return;

        var knowledge = EnsureComp<LanguageKnowledgeComponent>(knowledgeEnt.Value);
        // Goobstation edit end

        EnsureComp<LanguageSpeakerComponent>(uid, out var speaker);

        if (addSpoken && !knowledge.SpokenLanguages.Contains(language))
            knowledge.SpokenLanguages.Add(language);

        if (addUnderstood && !knowledge.UnderstoodLanguages.Contains(language))
            knowledge.UnderstoodLanguages.Add(language);

        UpdateEntityLanguages((uid, speaker));
    }

    /// <summary>
    ///     Removes a language from the respective lists of intrinsically known languages of the given entity.
    /// </summary>
    public void RemoveLanguage(
        EntityUid ent, // Goobstation edit
        ProtoId<LanguagePrototype> language,
        bool removeSpoken = true,
        bool removeUnderstood = true)
    {
        // Goobstation edit start
        if (!_knowledge.TryGetKnowledgeUnit(ent, LanguageKnowledgeId, out var knowledgeEnt))
            return;
        var knowledge = EnsureComp<LanguageKnowledgeComponent>(knowledgeEnt.Value);
        // Goobstation edit end

        if (removeSpoken)
            knowledge.SpokenLanguages.Remove(language); // Goobstation edit

        if (removeUnderstood)
            knowledge.UnderstoodLanguages.Remove(language); // Goobstation edit

        // We don't ensure that the entity has a speaker comp. If it doesn't... Well, woe be the caller of this method.
        UpdateEntityLanguages(ent);
    }

    /// <summary>
    ///   Ensures the given entity has a valid language as its current language.
    ///   If not, sets it to the first entry of its SpokenLanguages list, or universal if it's empty.
    /// </summary>
    /// <returns>True if the current language was modified, false otherwise.</returns>
    public bool EnsureValidLanguage(Entity<LanguageSpeakerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!ent.Comp.SpokenLanguages.Contains(ent.Comp.CurrentLanguage))
        {
            ent.Comp.CurrentLanguage = ent.Comp.SpokenLanguages.FirstOrDefault(UniversalPrototype);
            RaiseLocalEvent(ent, new LanguagesUpdateEvent());
            Dirty(ent);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Immediately refreshes the cached lists of spoken and understood languages for the given entity.
    /// </summary>
    public void UpdateEntityLanguages(Entity<LanguageSpeakerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var ev = new DetermineEntityLanguagesEvent();
        // We add the intrinsically known languages first so other systems can manipulate them easily
        // Goobstation edit start
        //if (TryComp<LanguageGrantComponent>(ent, out var knowledge))
        //{
        //    foreach (var spoken in knowledge.SpokenLanguages)
        //        ev.SpokenLanguages.Add(spoken);
        //
        //    foreach (var understood in knowledge.UnderstoodLanguages)
        //       ev.UnderstoodLanguages.Add(understood);
        //}
        if (_knowledge.TryEnsureKnowledgeUnit(ent.Owner, LanguageKnowledgeId, out var knowledgeEnt)
            && TryComp(knowledgeEnt, out LanguageKnowledgeComponent? languageKnowledge))
        {
            foreach (var spoken in languageKnowledge.SpokenLanguages)
                ev.SpokenLanguages.Add(spoken);

            foreach (var understood in languageKnowledge.UnderstoodLanguages)
                ev.UnderstoodLanguages.Add(understood);
        }
        // Goobstation edit end

        RaiseLocalEvent(ent, ref ev);

        ent.Comp.SpokenLanguages.Clear();
        ent.Comp.UnderstoodLanguages.Clear();

        ent.Comp.SpokenLanguages.AddRange(ev.SpokenLanguages);
        ent.Comp.UnderstoodLanguages.AddRange(ev.UnderstoodLanguages);

        // If EnsureValidLanguage returns true, it also raises a LanguagesUpdateEvent, so we try to avoid raising it twice in that case.
        if (!EnsureValidLanguage(ent))
            RaiseLocalEvent(ent, new LanguagesUpdateEvent());

        Dirty(ent);
    }

    #endregion
}
