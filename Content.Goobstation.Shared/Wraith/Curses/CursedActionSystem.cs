using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
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
        var attemptEv = new AttemptCurseEvent();
        RaiseLocalEvent(args.Target, ref attemptEv);

        if (attemptEv.Cancelled)
            return;

        // Add the curseHolder component and the new curse on the target
        EnsureComp<CurseHolderComponent>(args.Target);

        var curseApply = new CurseAppliedEvent(args.Curse);
        RaiseLocalEvent(args.Target, ref curseApply);

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
    }

    #region Cancel Events
    private void OnSiliconAttempt(Entity<SiliconComponent> ent, ref AttemptCurseEvent args)
    {
        // popup here
        args.Cancelled = true;
    }

    private void OnAttemptCurseImmune(Entity<CurseImmuneComponent> ent, ref AttemptCurseEvent args)
    {
        // popup here
        args.Cancelled = true;
    }
    #endregion
}
