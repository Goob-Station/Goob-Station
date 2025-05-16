using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Revolutionary.Components;


namespace Content.Shared.Revolutionary;

public sealed class RevolutionaryConverterSystem : EntitySystem
{
    private const string RevConvertSpeechBaseKey = "revolutionary-converter-speech-";

    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    private List<string> _speechLocalizationKeys = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevolutionaryConverterComponent, RevolutionaryConverterDoAfterEvent>(OnConvertDoAfter);
        SubscribeLocalEvent<RevolutionaryConverterComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<RevolutionaryConverterComponent, AfterInteractEvent>(OnConverterAfterInteract);

        var i = 1;
        while (Loc.HasString($"{RevConvertSpeechBaseKey}{i}"))
        {
            _speechLocalizationKeys.Add($"{RevConvertSpeechBaseKey}{i}");
            i++;
        }
    }

    private void OnUseInHand(Entity<RevolutionaryConverterComponent> ent, ref UseInHandEvent args)
    {
        if(_speechLocalizationKeys == null || _speechLocalizationKeys.Count == 0)
            return;
            
        var message = _speechLocalizationKeys[System.Random.Shared.Next(_speechLocalizationKeys.Count)];
        _chat.TrySendInGameICMessage(args.User, Loc.GetString(message), InGameICChatType.Speak, hideChat: false, hideLog: false);

        args.Handled = true;
    }

    public void OnConvertDoAfter(Entity<RevolutionaryConverterComponent> entity, ref RevolutionaryConverterDoAfterEvent args)
    {
        if (args.Target == null || args.Cancelled)
            return;

        var ev = new AfterConvertedEvent(args.Target!.Value, args.User, args.Used);
        RaiseLocalEvent(args.User, ref ev);

        if (args.Used != null)
            RaiseLocalEvent(args.Used.Value, ref ev);
    }

    public void OnConverterAfterInteract(Entity<RevolutionaryConverterComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (args.Target is not { Valid: true } target || !HasComp<MobStateComponent>(target) || !HasComp<HeadRevolutionaryComponent>(args.User))
            return;

        ConvertDoAfter(entity, target, args.User);
        args.Handled = true;
    }

    private void ConvertDoAfter(Entity<RevolutionaryConverterComponent> converter, EntityUid target, EntityUid user)
    {
        if (user == target)
            return;

        if(_speechLocalizationKeys != null && _speechLocalizationKeys.Count > 0)
        {
            var message = _speechLocalizationKeys[System.Random.Shared.Next(_speechLocalizationKeys.Count)];
            _chat.TrySendInGameICMessage(user, Loc.GetString(message), InGameICChatType.Speak, hideChat: false, hideLog: false);
         }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, converter.Comp.ConversionDuration, new RevolutionaryConverterDoAfterEvent(), converter.Owner, target: target, used: converter.Owner)
        {
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
            BreakOnDamage = true,
            NeedHand = true,
            BreakOnHandChange = false,
        });
    }

    /// <summary>
    /// Called after a converter is used via melee on another person to check for rev conversion.
    /// Raised on the user of the converter, the target hit by the converter, and the converter used.
    /// </summary>
    [ByRefEvent]
    public readonly struct AfterConvertedEvent
    {
        public readonly EntityUid Target;
        public readonly EntityUid? User;
        public readonly EntityUid? Used;

        public AfterConvertedEvent(EntityUid target, EntityUid? user, EntityUid? used)
        {
            Target = target;
            User = user;
            Used = used;
        }
    }
}
