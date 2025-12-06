using Content.Server._EinsteinEngines.Language;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._EinsteinEngines.Language.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio.Systems;
using Content.Goobstation.Shared._Europa;

namespace Content.Goobstation.Server._Europa.Language;

/// <summary>
/// This handles learning languages on use of items
/// </summary>

public sealed class LanguageLearnSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly LanguageSystem _language = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguageLearnComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<LanguageLearnComponent, LanguageLearnDoAfterEvent>(OnUsed, after: new []{typeof(DoAfterSystem)});
        SubscribeLocalEvent<LanguageLearnComponent, ExaminedEvent>(OnExamine);
    }


    private void OnUseInHand(EntityUid uid, LanguageLearnComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var usesRemaining = component.GetUsesRemaining();

        if (usesRemaining <= 0)
        {
            _popup.PopupEntity(Loc.GetString("language-item-no-uses"), uid, args.User);
            return;
        }

        args.Handled = true;

        var Event = new LanguageLearnDoAfterEvent();
        var Args = new DoAfterArgs(EntityManager, args.User, component.DoAfterDuration, Event, uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        _doAfterSystem.TryStartDoAfter(Args);
        _audio.PlayPvs(component.UseSound, uid);
    }

    private void OnUsed(EntityUid uid, LanguageLearnComponent component, LanguageLearnDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<LanguageKnowledgeComponent>(args.User, out var languageKnowledge))
            return;

        bool learnedSomething = false;

        if (!HasComp<UniversalLanguageSpeakerComponent>(args.User) && component.Languages.Contains("Universal"))
        {
            EnsureComp<UniversalLanguageSpeakerComponent>(args.User);
            learnedSomething = true;
        }
        foreach (var language in component.Languages)
        {
            if (language == "Universal")
                continue;

            if (languageKnowledge.SpokenLanguages.Contains(language))
            {
                _popup.PopupEntity(Loc.GetString("language-item-already-knows"), uid, args.User);
                continue;
            }

            languageKnowledge.SpokenLanguages.Add(language);
            languageKnowledge.UnderstoodLanguages.Add(language);
            learnedSomething = true;
        }

        _language.UpdateEntityLanguages(args.User);
        _audio.PlayPvs(component.UseSound, uid);

        if (learnedSomething)
        {
            var usesRemaining = component.GetUsesRemaining();
            usesRemaining--;

            component.UsesRemaining = usesRemaining;
            Dirty(uid, component);

            if (component.DeleteAfterUse && usesRemaining <= 0)
            {
                EntityManager.QueueDeleteEntity(uid);
            }
        }
    }

    private void OnExamine(EntityUid uid, LanguageLearnComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var usesRemaining = component.GetUsesRemaining();

        args.PushMarkup(Loc.GetString("language-item-uses-remaining", ("uses", usesRemaining)));
    }
}
