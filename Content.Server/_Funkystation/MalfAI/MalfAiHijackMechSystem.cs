// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Shared.CombatMode;
using Content.Shared.Intellicard;
using Content.Shared.Mobs;
using Content.Shared.Database;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Containers;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI hijacking a mech for 2 minutes by transferring its mind into the
/// mech entity. The brain stays behind in its holder, so the crew can still intellicard
/// the AI while it joyrides.
/// </summary>
public sealed class MalfAiHijackMechSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedMechSystem _mech = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    private const float HijackDurationSeconds = 120f;
    private static readonly TimeSpan PilotStun = TimeSpan.FromSeconds(5);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiHijackMechActionEvent>(OnHijackMech);
        SubscribeLocalEvent<MalfAiMechHijackComponent, MalfAiReturnToCoreActionEvent>(OnReturnFromMech);
        SubscribeLocalEvent<MalfAiMechHijackComponent, EntInsertedIntoContainerMessage>(OnPilotEntered);
        SubscribeLocalEvent<MalfAiMarkerComponent, EntGotInsertedIntoContainerMessage>(OnBrainInserted);
        SubscribeLocalEvent<MalfAiMarkerComponent, MobStateChangedEvent>(OnBrainMobState);
        SubscribeLocalEvent<MalfAiMarkerComponent, EntityTerminatingEvent>(OnBrainTerminating);
    }

    /// <summary>
    /// A dying or deleted brain ends the hijack so the player dies with it instead of
    /// staying stranded in the mech.
    /// </summary>
    private void OnBrainMobState(Entity<MalfAiMarkerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            EndHijackForBrain(ent.Owner);
    }

    private void OnBrainTerminating(Entity<MalfAiMarkerComponent> ent, ref EntityTerminatingEvent args)
    {
        EndHijackForBrain(ent.Owner);
    }

    private void EndHijackForBrain(EntityUid brain)
    {
        var query = EntityQueryEnumerator<MalfAiMechHijackComponent>();
        while (query.MoveNext(out var mech, out var hijack))
        {
            if (hijack.Brain != brain)
                continue;

            ReturnFromHijack(mech, hijack);
            break;
        }
    }

    /// <summary>
    /// Carding the brain yanks the mind straight back out of the mech.
    /// </summary>
    private void OnBrainInserted(Entity<MalfAiMarkerComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!HasComp<IntellicardComponent>(args.Container.Owner))
            return;

        var query = EntityQueryEnumerator<MalfAiMechHijackComponent>();
        while (query.MoveNext(out var mech, out var hijack))
        {
            if (hijack.Brain != ent.Owner)
                continue;

            ReturnFromHijack(mech, hijack);
            break;
        }
    }

    /// <summary>
    /// A crew member climbing into the pilot seat wrestles control back from the AI.
    /// </summary>
    private void OnPilotEntered(Entity<MalfAiMechHijackComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<MechComponent>(ent.Owner, out var mech) || args.Container.ID != mech.PilotSlotId)
            return;

        var brain = ent.Comp.Brain;
        ReturnFromHijack(ent.Owner, ent.Comp);

        if (!Deleted(brain))
            _popup.PopupEntity(Loc.GetString("malfai-hijack-pilot-retook-control"), brain, brain, PopupType.LargeCaution);
    }

    private void OnHijackMech(Entity<MalfAiMarkerComponent> ent, ref MalfAiHijackMechActionEvent args)
    {
        if (args.Handled)
            return;

        var brain = ent.Owner;

        if (!TryComp<MechComponent>(args.Target, out var mech))
        {
            _popup.PopupEntity(Loc.GetString("malfai-hijack-not-mech"), brain, brain);
            return;
        }

        if (mech.Broken)
        {
            _popup.PopupEntity(Loc.GetString("malfai-hijack-mech-broken"), brain, brain);
            return;
        }

        if (HasComp<MalfAiMechHijackComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("malfai-hijack-already-hijacking"), brain, brain);
            return;
        }

        if (!_mind.TryGetMind(brain, out var mindId, out _))
            return;

        var target = args.Target;

        // Eject and stun the current pilot.
        if (mech.PilotSlot.ContainedEntity is { } currentPilot)
        {
            _mech.TryEject(target, mech);
            _stun.TryAddParalyzeDuration(currentPilot, PilotStun);
        }

        // The mind moves into the mech; the brain stays cardable in its holder.
        _mind.TransferTo(mindId, target);

        var hijack = EnsureComp<MalfAiMechHijackComponent>(target);
        hijack.Brain = brain;

        EntityUid? returnAction = null;
        _actions.AddAction(target, ref returnAction, "ActionMalfAiReturnToCore");
        hijack.ReturnAction = returnAction;

        // Gun and melee relays resolve through MechPilotComponent; pointing the mech at
        // itself lets the AI use the selected equipment like a pilot would.
        if (!HasComp<MechPilotComponent>(target))
        {
            var pilot = EnsureComp<MechPilotComponent>(target);
            pilot.Mech = target;
            Dirty(target, pilot);
            hijack.AddedPilotComp = true;
        }

        if (!HasComp<CombatModeComponent>(target))
        {
            EnsureComp<CombatModeComponent>(target);
            hijack.AddedCombatComp = true;
        }

        // Equipment cycling and mech UI: mental mirrors of the pilot actions, because the
        // mech entity itself fails the regular interaction/consciousness action blockers.
        EntityUid? cycleAction = null;
        _actions.AddAction(target, ref cycleAction, "ActionMalfAiMechCycle");
        hijack.CycleAction = cycleAction;

        EntityUid? uiAction = null;
        _actions.AddAction(target, ref uiAction, "ActionMalfAiMechUi");
        hijack.UiAction = uiAction;

        _adminLog.Add(LogType.Action, LogImpact.High,
            $"Malf AI {ToPrettyString(brain)} hijacked mech {ToPrettyString(target)}");

        // Auto-return after the hijack duration.
        var mechCapture = target;
        Timer.Spawn(TimeSpan.FromSeconds(HijackDurationSeconds), () =>
        {
            if (!Deleted(mechCapture) && TryComp<MalfAiMechHijackComponent>(mechCapture, out var h))
                ReturnFromHijack(mechCapture, h);
        });

        args.Handled = true;
    }

    private void OnReturnFromMech(Entity<MalfAiMechHijackComponent> ent, ref MalfAiReturnToCoreActionEvent args)
    {
        if (args.Handled)
            return;

        ReturnFromHijack(ent.Owner, ent.Comp);
        args.Handled = true;
    }

    /// <summary>
    /// Ends the hijack: the mind returns into the AI brain, wherever it is now
    /// (core, APC or an intellicard if the crew carded it in the meantime).
    /// </summary>
    public void ReturnFromHijack(EntityUid mech, MalfAiMechHijackComponent hijack)
    {
        if (!Deleted(hijack.Brain) && _mind.TryGetMind(mech, out var mindId, out _))
            _mind.TransferTo(mindId, hijack.Brain);

        if (hijack.AddedCombatComp)
            RemComp<CombatModeComponent>(mech);

        if (hijack.AddedPilotComp)
            RemComp<MechPilotComponent>(mech);

        if (hijack.CycleAction is { } cycleAction)
            _actions.RemoveAction(cycleAction);

        if (hijack.UiAction is { } uiAction)
            _actions.RemoveAction(uiAction);

        if (hijack.ReturnAction is { } returnAction)
            _actions.RemoveAction(returnAction);

        RemComp<MalfAiMechHijackComponent>(mech);

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"Malf AI returned from mech hijack of {ToPrettyString(mech)}");
    }
}
