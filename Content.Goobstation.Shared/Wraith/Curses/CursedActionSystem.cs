using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Curses;

/// <summary>
/// This handles applying curses to an entity.
/// This system also handles entities that are not allowed to get curses
/// </summary>
public sealed class CursedActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const int MaxCursesBeforeFinal = 4;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ApplyCurseActionEvent>(OnApplyCurseAction);

        SubscribeLocalEvent<SiliconComponent, AttemptCurseEvent>(OnSiliconAttempt);
        SubscribeLocalEvent<CurseImmuneComponent, AttemptCurseEvent>(OnAttemptCurseImmune);
    }

    private void OnApplyCurseAction(ApplyCurseActionEvent args)
    {
        if (args.Curse == null)
            return;

        var attemptEv = new AttemptCurseEvent(args.Performer);
        RaiseLocalEvent(args.Target, ref attemptEv);

        if (attemptEv.Cancelled)
            return;

        // Add the curseHolder component and the new curse on the target
        var curseHolder = EnsureComp<CurseHolderComponent>(args.Target);

        if (args.RequireAllCurses)
        {
            if (curseHolder.ActiveCurses.Count < MaxCursesBeforeFinal)
            {
                _popup.PopupClient(Loc.GetString("curse-fail-require-all"), args.Performer, args.Performer);
                return;
            }
        }

        var curseApply = new CurseAppliedEvent(args.Curse.Value, args.Performer);
        RaiseLocalEvent(args.Target, ref curseApply);

        if (curseApply.Cancelled)
            return;

        if (args.Popup.HasValue)
            _popup.PopupClient(Loc.GetString(args.Popup.Value), args.Performer, args.Performer, PopupType.Medium);

        // play curse sound if it exists
        if (args.CurseSound != null && _netManager.IsServer)
            _audio.PlayEntity(args.CurseSound, args.Target, args.Target);

        // Reset timers on all curses for the user
        if (!TryComp<ActionsComponent>(args.Performer, out var actions))
            return;

        foreach (var action in actions.Actions)
        {
            if (!HasComp<CurseActionComponent>(action))
                continue;

            _actions.StartUseDelay(action);
        }

        args.Handled = true;
    }

    #region Cancel Events
    private void OnSiliconAttempt(Entity<SiliconComponent> ent, ref AttemptCurseEvent args)
    {
        _popup.PopupClient(Loc.GetString("curse-fail-robot"), args.Curser, args.Curser);
        args.Cancelled = true;
    }

    private void OnAttemptCurseImmune(Entity<CurseImmuneComponent> ent, ref AttemptCurseEvent args)
    {
        _popup.PopupClient(Loc.GetString("curse-immune-fail"), args.Curser, args.Curser);
        args.Cancelled = true;
    }
    #endregion
}
