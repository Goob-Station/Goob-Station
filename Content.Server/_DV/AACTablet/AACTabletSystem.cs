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
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Radio.Components;
using Content.Shared.Radio.EntitySystems;
using Robust.Shared.Containers;


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
        //goob languages start
        SubscribeLocalEvent<AACTabletComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AACTabletComponent, InteractUsingEvent>(OnLanguageKeyInsert);
        SubscribeLocalEvent<AACTabletComponent, EncryptionKeySystem.EncryptionRemovalFinishedEvent>(OnLanguageKeyRemove);
        SubscribeLocalEvent<AACTabletComponent, GotEquippedHandEvent>(OnPickup);
        SubscribeNetworkEvent<AACTabletLanguagesRefreshedEvent>(OnLanguageSelected);
        //goob languages end
    }

    // Goob edit start, languages.
    private void OnStartup(EntityUid uid, AACTabletComponent comp, ComponentStartup  args)
    {
        RefreshLanguages(uid, comp);
    }

    private void OnPickup(EntityUid uid, AACTabletComponent comp, GotEquippedHandEvent args)
    {
        if (!TryComp<LanguageSpeakerComponent>(uid, out var speaker))
            return;
        var current = comp.CurrentLanguage;
        if (!speaker.SpokenLanguages.ToString()!.Contains(comp.CurrentLanguage) || comp.CurrentLanguage == null)
        {
            current = speaker.SpokenLanguages.FirstOrDefault();
        }

        var netuid = GetNetEntity(uid);
        var ev = new AACTabletLanguagesRefreshedEvent(netuid, speaker.SpokenLanguages, current);
        RaiseNetworkEvent(ev, args.User);
    }

    private void OnLanguageSelected(AACTabletLanguagesRefreshedEvent ev, EntitySessionEventArgs args)
    {
        if (!TryComp<LanguageSpeakerComponent>(GetEntity(ev.Tablet), out var speaker))
            return;
        speaker.
    }

    private void RefreshLanguages(EntityUid ent, AACTabletComponent comp)
    {
        if (
            !HasComp<EncryptionKeyHolderComponent>(ent) ||
            !TryComp<ContainerFillComponent>(ent, out var container) ||
            !TryComp<LanguageSpeakerComponent>(ent, out var speaker) ||
            !container.Containers.TryGetValue(EncryptionKeyHolderComponent.KeyContainerName,
                out var containedLanguageKeyList)
        )
            return;

        foreach (var languageKey in containedLanguageKeyList)
        {
            _prototype.TryIndex<EntityPrototype>(languageKey,
                out var languageKeyProto); // We're getting the languages from the key prototype NOT the tablet.
            if (languageKeyProto == null ||
                !languageKeyProto.Components.TryGetValue(_compFactory.GetComponentName<LanguageKnowledgeComponent>(), out var reg)) // What the fuck is this horseshit i miss TryGetComponent
                continue;
            var innerKeyLanguageComp = (LanguageKnowledgeComponent) reg.Component;
            _language.ClearEntityLanguages(ent);
            _language.AddLanguage(ent, innerKeyLanguageComp.SpokenLanguages.FirstOrDefault());
            _language.UpdateEntityLanguages(ent);
        }

        // after change, if it still has the language that was previously selected, we keep it selected.
        if (speaker.SpokenLanguages.ToString()!.Contains(comp.CurrentLanguage) && comp.CurrentLanguage != null)
        {
            speaker.CurrentLanguage = comp.CurrentLanguage;
            _language.UpdateEntityLanguages(ent);
            return;
        }

        speaker.CurrentLanguage = speaker.SpokenLanguages.FirstOrDefault().ToString();
        _language.UpdateEntityLanguages(ent);
    }

    private void OnLanguageKeyInsert(EntityUid ent, AACTabletComponent comp, InteractUsingEvent args)
    {
        if (!HasComp<LanguageKnowledgeComponent>(args.Used)) // we dont care about radio keys.
            return;
        RefreshLanguages(args.Target, comp);
    }

    private void OnLanguageKeyRemove(EntityUid ent, AACTabletComponent comp, EncryptionKeySystem.EncryptionRemovalFinishedEvent args)
    {
        if (args.Target == null)
            return;
        RefreshLanguages(args.Target.Value, comp);
    }

    // Goob edit end, language

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
