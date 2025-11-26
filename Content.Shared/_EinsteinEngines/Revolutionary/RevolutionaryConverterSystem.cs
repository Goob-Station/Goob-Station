using Content.Shared._EinsteinEngines.Language.Components;
using Content.Shared._EinsteinEngines.Language.Systems;
using Content.Shared._EinsteinEngines.Revolutionary.Components;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Chat;
using Content.Shared.Dataset;
using Content.Shared.DoAfter;
using Content.Shared.Flash;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Random.Helpers;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._EinsteinEngines.Revolutionary;

public sealed class RevolutionaryConverterSystem : EntitySystem
{
    private static readonly ProtoId<LocalizedDatasetPrototype> RevConvertSpeechProto = "RevolutionaryConverterSpeech";

    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedLanguageSystem _language = default!;
    [Dependency] private readonly SharedChargesSystem _chargesSystem = default!;
    [Dependency] private readonly SharedFlashSystem _flash = default!;

    private LocalizedDatasetPrototype? _speechLocalization;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevolutionaryConverterComponent, RevolutionaryConverterDoAfterEvent>(OnConvertDoAfter);
        SubscribeLocalEvent<RevolutionaryConverterComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<RevolutionaryConverterComponent, AfterInteractEvent>(OnConverterAfterInteract);

        _speechLocalization = _prototypeManager.Index<LocalizedDatasetPrototype>(RevConvertSpeechProto);
    }

    private void OnUseInHand(Entity<RevolutionaryConverterComponent> ent, ref UseInHandEvent args)
    {
        if (!SpeakPropaganda(ent, args.User))
            return;

        args.Handled = true;
    }

    private bool SpeakPropaganda(Entity<RevolutionaryConverterComponent> conversionToolEntity, EntityUid user)
    {
        if(_speechLocalization == null
            || _speechLocalization.Values.Count == 0
            || conversionToolEntity.Comp.Silent)
            return false;

        var message = _random.Pick(_speechLocalization);
        _chat.TrySendInGameICMessage(user, Loc.GetString(message), InGameICChatType.Speak, hideChat: false, hideLog: false);
        return true;
    }

    public void OnConvertDoAfter(Entity<RevolutionaryConverterComponent> entity, ref RevolutionaryConverterDoAfterEvent args)
    {
        if (args.Target == null
            || args.Cancelled
            || args.Used == null
            || args.Target == null)
            return;

        ConvertTarget(args.Used.Value, args.Target.Value, args.User);
    }

    public void ConvertTarget(EntityUid used, EntityUid targetConvertee, EntityUid user)
    {
        var ev = new AfterRevolutionaryConvertedEvent(targetConvertee, user, used);
        RaiseLocalEvent(user, ref ev);
        RaiseLocalEvent(used, ref ev);
    }

    public void OnConverterAfterInteract(Entity<RevolutionaryConverterComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled
            || !args.Target.HasValue
            || !args.CanReach
            || (entity.Comp.ConsumesCharges > 0
            && !_chargesSystem.TryUseCharges(entity.Owner, entity.Comp.ConsumesCharges)))
            return;

        if (entity.Comp.ApplyFlashEffect)
        {
            _flash.Flash(args.Target.Value, args.User, entity.Owner, entity.Comp.FlashDuration, entity.Comp.SlowToOnFlashed, melee: true);

            bool hasChargesLeft = entity.Comp.ConsumesCharges <= 0 || _chargesSystem.HasCharges(entity.Owner, entity.Comp.ConsumesCharges);
            _appearance.SetData(entity.Owner, FlashVisuals.Flashing, hasChargesLeft);
            _appearance.SetData(entity.Owner, FlashVisuals.Burnt, !hasChargesLeft);
        }

        if (args.Target is not { Valid: true } target
            || !HasComp<MobStateComponent>(target)
            || !HasComp<HeadRevolutionaryComponent>(args.User))
            return;

        ConvertDoAfter(entity, target, args.User);
        args.Handled = true;
    }

    private void ConvertDoAfter(Entity<RevolutionaryConverterComponent> converter, EntityUid target, EntityUid user)
    {
        if (user == target)
            return;

        if (SpeakPropaganda(converter, user)
            // Note: this check is skipped if the speaker speaks lines and somehow doesn't have a languageSpeaker component.
            && EntityManager.TryGetComponent<LanguageSpeakerComponent>(user, out var speakerComponent)) // returns true if the chosen conversion method uses a spoken line of text
        {
            //check if spoken language can be understood by target
            if (!_language.CanUnderstand(target, speakerComponent.CurrentLanguage))
                return; //the target does not understand the speaker's language, so the conversion fails
        }

        if (converter.Comp.ConversionDuration > TimeSpan.Zero)
        {
            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                user,
                converter.Comp.ConversionDuration,
                new RevolutionaryConverterDoAfterEvent(),
                converter.Owner,
                target: target,
                used: converter.Owner,
                showTo: converter.Owner)
            {
                Hidden = !converter.Comp.VisibleDoAfter,
                BreakOnMove = false,
                BreakOnWeightlessMove = false,
                BreakOnDamage = true,
                NeedHand = true,
                BreakOnHandChange = false,
            });
        }
        else
            ConvertTarget(converter.Owner, target, user);
    }
}

/// <summary>
/// Called after a converter is used on another person to check for rev conversion.
/// Raised on the user of the converter, the target hit by the converter, and the converter used.
/// </summary>
[ByRefEvent]
public readonly struct AfterRevolutionaryConvertedEvent(EntityUid target, EntityUid? user, EntityUid? used)
{
    public readonly EntityUid Target = target;
    public readonly EntityUid? User = user;
    public readonly EntityUid? Used = used;
}
