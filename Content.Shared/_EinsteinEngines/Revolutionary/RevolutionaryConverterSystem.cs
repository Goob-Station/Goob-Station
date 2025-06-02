using Content.Shared._EinsteinEngines.Revolutionary.Components;
using Content.Shared.Chat;
using Content.Shared.Dataset;
using Content.Shared.DoAfter;
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

    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

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
            || args.Cancelled)
            return;

        var ev = new AfterRevolutionaryConvertedEvent(args.Target!.Value, args.User, args.Used);
        RaiseLocalEvent(args.User, ref ev);

        if (args.Used != null)
            RaiseLocalEvent(args.Used.Value, ref ev);
    }

    public void OnConverterAfterInteract(Entity<RevolutionaryConverterComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled
            || !args.CanReach)
            return;

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

        SpeakPropaganda(converter, user);

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
}

/// <summary>
/// Called after a converter is used via melee on another person to check for rev conversion.
/// Raised on the user of the converter, the target hit by the converter, and the converter used.
/// </summary>
[ByRefEvent]
public readonly struct AfterRevolutionaryConvertedEvent(EntityUid target, EntityUid? user, EntityUid? used)
{
    public readonly EntityUid Target = target;
    public readonly EntityUid? User = user;
    public readonly EntityUid? Used = used;
}
