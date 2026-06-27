using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Administration.Components;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Abilities.Mime;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;

namespace Content.Goobstation.Server.Mimery;

public sealed class MimeFuckYouSystem : EntitySystem
{
    [Dependency] private readonly LanguageSystem _languages = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly ThunderstrikeSystem _thunderstrikeSystem = default!;
    [Dependency] private readonly StunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MimePowersComponent, SpeakAttemptEvent>(OnSpeakAttempt);
    }

    private void OnSpeakAttempt(Entity<MimePowersComponent> ent, ref SpeakAttemptEvent args)
    {
        if (ent.Comp.VowBroken)
            return;

        var language = _languages.GetLanguage(ent.Owner);
        if (language.SpeechOverride.RequireSpeech) // handled in MutingSystem
            return;

        // you think youre fucking clever

        ent.Comp.NonverbalViolationCount++;

        int violationCount = ent.Comp.NonverbalViolationCount;
        int maxViolationCount = ent.Comp.NonverbalMaxViolationCount;
        int youAreDone = ent.Comp.NonverbalViolationCountYouAreDone;

        if (violationCount < maxViolationCount) // warning
        {
            _popupSystem.PopupEntity(Loc.GetString("lazy-mime-" + violationCount.ToString()), ent, PopupType.SmallCaution);
            args.Cancel();
        }
        else if (violationCount == maxViolationCount) // punishment
        {
            _popupSystem.PopupEntity(Loc.GetString("lazy-mime-too-much"), ent, PopupType.LargeCaution);
            _stunSystem.TryAddParalyzeDuration(ent, TimeSpan.FromSeconds(5));
            EnsureComp<KillSignComponent>(ent);
        }
        else if (violationCount == youAreDone) // you will grow complacent
        {
            _popupSystem.PopupCoordinates(Loc.GetString("lazy-mime-you-are-done"), Transform(ent).Coordinates, PopupType.LargeCaution);
            _thunderstrikeSystem.Smite(ent, true); // bye
        }
    }
}