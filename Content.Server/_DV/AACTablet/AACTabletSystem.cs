using System.Linq;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Chat.Systems;
using Content.Server.Speech.Components;
using Content.Shared._DV.AACTablet;
using Content.Shared._EinsteinEngines.Language.Components;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Chat;
using Content.Shared.Containers;
using Content.Shared.Item;
using Content.Shared.Radio.Components;
using Robust.Shared.Containers;
using Robust.Shared.Utility;


namespace Content.Server._DV.AACTablet;

public sealed class AACTabletSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly LanguageSystem _language = default!;

    private readonly List<string> _localisedPhrases = [];

    public const int MaxPhrases = 10; // no writing novels

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AACTabletComponent, AACTabletSendPhraseMessage>(OnSendPhrase);
    }

    private void OnStartup(Entity<AACTabletComponent> ent, ContainerFillComponent container,
        LanguageSpeakerComponent speaker, LanguageKnowledgeComponent outerKeyLanguageComp, ComponentStartup start)
    {
        var comp = EnsureComp<LanguageSpeakerComponent>(ent); // Ensure they can speak language before adding language.
        if (!HasComp<EncryptionKeyHolderComponent>(ent) ||
            !container.Containers.TryGetValue(EncryptionKeyHolderComponent.KeyContainerName,
                out var containedLanguageKeyList)
           )
            return;
        RefreshLanguages(ent, containedLanguageKeyList, speaker);

        if (outerKeyLanguageComp.SpokenLanguages.Contains("Universal")) // assume no key.
            return;


    }

    private void RefreshLanguages(Entity<AACTabletComponent> ent, List<string>? containedLanguageKeyList, LanguageSpeakerComponent speaker)
    {
        if (containedLanguageKeyList == null)
            return;

        foreach (var languageKey in containedLanguageKeyList)
        {
            _prototype.TryIndex<EntityPrototype>(languageKey, out var languageKeyProto); // We're getting the languages from the key prototype NOT the tablet.
            if (languageKeyProto == null ||
                !languageKeyProto.Components.TryGetValue(
                    _compFactory
                        .GetComponentName<
                            LanguageKnowledgeComponent>(), // What the fuck is this horseshit i miss TryGetComponent
                    out var reg))
                continue;
            var innerKeyLanguageComp = (LanguageKnowledgeComponent)reg.Component;
            innerKeyLanguageComp.SpokenLanguages.Clear();
            _language.AddLanguage(ent,
                innerKeyLanguageComp.SpokenLanguages
                    .ToString()!, // if you give something a languagecomp without language thats your problem.
                true, false); // we speak not listen.
        }

        speaker.CurrentLanguage =
            speaker.SpokenLanguages.FirstOrDefault();
        _language.UpdateEntityLanguages(ent.Owner);
    }

    private void OnLanguageKeyInsert(Entity<AACTabletComponent> ent, Entity<LanguageKnowledgeComponent> insertedKey)
    {
        if (HasComp<EncryptionKeyComponent>(insertedKey)) // we dont care about radio keys.
            return;


        var langContainerFill = _container.GetAllContainers(ent);
        if (!_container.CanInsert(insertedKey), langContainerFill, )
            return;
    }

private void OnSendPhrase(Entity<AACTabletComponent> ent, ref AACTabletSendPhraseMessage message)
    {
        if (ent.Comp.NextPhrase > _timing.CurTime || message.PhraseIds.Count > MaxPhrases)
            return;

        var senderName = Identity.Entity(message.Actor, EntityManager);
        var speakerName = Loc.GetString("speech-name-relay",
            ("speaker", Name(ent)),
            ("originalName", senderName));

        _localisedPhrases.Clear();
        foreach (var phraseProto in message.PhraseIds)
        {
            if (_prototype.TryIndex(phraseProto, out var phrase))
            {
                // Ensures each phrase is capitalised to maintain common AAC styling
                _localisedPhrases.Add(_chat.SanitizeMessageCapital(Loc.GetString(phrase.Text)));
            }
        }

        if (_localisedPhrases.Count <= 0)
            return;

        EnsureComp<VoiceOverrideComponent>(ent).NameOverride = speakerName;

        _chat.TrySendInGameICMessage(ent,
            string.Join(" ", _localisedPhrases),
            InGameICChatType.Speak,
            hideChat: false,
            nameOverride: speakerName);

        var curTime = _timing.CurTime;
        ent.Comp.NextPhrase = curTime + ent.Comp.Cooldown;
    }
}
