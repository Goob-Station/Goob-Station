using Content.Server.Chat.Managers;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Server.Player;
using Robust.Shared.Timing;
using Robust.Shared.Player;

namespace Content.Server.Abilities.Psionics;

public sealed class AssayPowerSystem : EntitySystem
{
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicComponent, AssayPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<PsionicComponent, AssayDoAfterEvent>(OnDoAfter);
    }

    /// <summary>
    ///     This power activates when scanning any entity, displaying to the player's chat window a variety of psionic related statistics about the target.
    /// </summary>
    private void OnPowerUsed(EntityUid uid, PsionicComponent psionic, AssayPowerActionEvent args)
    {
        if (!_psionics.OnAttemptPowerUse(args.Performer, args.Target, "assay", true)
            || psionic.DoAfter is not null)
            return;

        var ev = new AssayDoAfterEvent(_gameTiming.CurTime, args.FontSize, args.FontColor);
        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, args.UseDelay - TimeSpan.FromSeconds(psionic.CurrentAmplification), ev, args.Performer, args.Target, args.Performer)
        {
            BlockDuplicate = true,
            BreakOnMove = true,
            BreakOnDamage = true,
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfterArgs, out var doAfterId))
            return;

        psionic.DoAfter = doAfterId;

        _popups.PopupEntity(Loc.GetString(args.PopupTarget, ("entity", args.Target)), args.Performer, PopupType.Medium);
        _audioSystem.PlayPvs(args.SoundUse, args.Performer, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
        _psionics.LogPowerUsed(args.Performer, args.PowerName, args.MinGlimmer, args.MaxGlimmer);
        args.Handled = true;
    }

    /// <summary>
    ///     Assuming the DoAfter wasn't canceled, the user wasn't mindbroken, and the target still exists, prepare the scan results!
    /// </summary>
    private void OnDoAfter(EntityUid uid, PsionicComponent component, AssayDoAfterEvent args)
    {
        if (component is null)
            return;
        component.DoAfter = null;

        var user = uid;
        var target = args.Target;
        if (target == null || args.Cancelled
            || !_playerManager.TryGetSessionByEntity(user, out var session))
            return;

        if (InspectSelf(uid, args, session))
            return;

        if (!TryComp<PsionicComponent>(target, out var targetPsionic))
        {
            var noPowers = Loc.GetString("no-powers", ("entity", target));
            _popups.PopupEntity(noPowers, user, user, PopupType.LargeCaution);

            // Incredibly spooky message for non-psychic targets.
            var noPowersFeedback = $"[font size={args.FontSize}][color={args.FontColor}]{noPowers}[/color][/font]";
            SendDescToChat(noPowersFeedback, session);
            return;
        }

        InspectOther(targetPsionic, args, session);
    }

    /// <summary>
    ///     This is a special use-case for scanning yourself with the power. The player receives a unique feedback message if they do so.
    ///     It however displays significantly less information when doing so. Consider this an intriguing easter egg.
    /// </summary>
    private bool InspectSelf(EntityUid uid, AssayDoAfterEvent args, ICommonSession session)
    {
        if (args.Target != args.User)
            return false;

        var user = uid;
        var target = args.Target;

        var assaySelf = Loc.GetString("assay-self", ("entity", target!.Value));
        _popups.PopupEntity(assaySelf, user, user, PopupType.LargeCaution);

        var assaySelfFeedback = $"[font size=20][color=#ff0000]{assaySelf}[/color][/font]";
        SendDescToChat(assaySelfFeedback, session);
        return true;
    }

    /// <summary>
    ///     If the target turns out to be a psychic, display their feedback messages in chat.
    /// </summary>
    private void InspectOther(PsionicComponent targetPsionic, AssayDoAfterEvent args, ICommonSession session)
    {
        var target = args.Target;
        var targetAmp = MathF.Round(targetPsionic.CurrentAmplification, 2).ToString("#.##");
        var targetDamp = MathF.Round(targetPsionic.CurrentDampening, 2).ToString("#.##");
        var targetPotentia = MathF.Round(targetPsionic.Potentia, 2).ToString("#.##");
        var message = $"[font size={args.FontSize}][color={args.FontColor}]{Loc.GetString("assay-body", ("entity", target!.Value), ("amplification", targetAmp), ("dampening", targetDamp), ("potentia", targetPotentia))}[/color][/font]";
        SendDescToChat(message, session);

        foreach (var feedback in targetPsionic.AssayFeedback)
        {
            var locale = Loc.GetString(feedback, ("entity", target!.Value));
            var feedbackMessage = $"[font size={args.FontSize}][color={args.FontColor}]{locale}[/color][/font]";
            SendDescToChat(feedbackMessage, session);
        }
    }

    private void SendDescToChat(string feedbackMessage, ICommonSession session)
    {
        _chatManager.ChatMessageToOne(
            ChatChannel.Emotes,
            feedbackMessage,
            feedbackMessage,
            EntityUid.Invalid,
            false,
            session.Channel);
    }
}
