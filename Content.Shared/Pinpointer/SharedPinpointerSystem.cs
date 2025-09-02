// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Shared.Pinpointer;

public abstract class SharedPinpointerSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] protected readonly EntityWhitelistSystem Whitelist = default!; // Goob edit
    [Dependency] private readonly SharedPopupSystem _popup = default!; // Goob edit

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PinpointerComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<PinpointerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PinpointerComponent, ExaminedEvent>(OnExamined);
    }

    /// <summary>
    ///     Set the target if capable
    /// </summary>
    private void OnAfterInteract(EntityUid uid, PinpointerComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target || args.Handled)
            return;

        if (!component.CanRetarget || component.IsActive)
            return;

        // Goob edit start: retargeting has a whitelist
        args.Handled = true;

        if (Whitelist.IsWhitelistFail(component.RetargetingWhitelist, target) ||
            Whitelist.IsBlacklistPass(component.RetargetingBlacklist, target))
        {
            return;
        }

        // TODO add doafter once the freeze is lifted
        // ignore can target multiple, because too hard to support
        component.Targets.Clear();
        component.Targets.Add(target);
        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):player} set target of {ToPrettyString(uid):pinpointer} to {ToPrettyString(target):target}");
        if (component.UpdateTargetName)
            component.TargetName = Identity.Name(target, EntityManager);

        _popup.PopupPredicted(Loc.GetString("pinpointer-link-success"), uid, args.User);
        // Goob edit end
    }

    /// <summary>
    ///     Set pinpointers target to track
    ///     Goob edit: If CanTargetMultiple is true in Pinpointer component, then it will be ADDED, not set
    /// </summary>
    public virtual void SetTarget(EntityUid uid, EntityUid? target, PinpointerComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        if (target == null || pinpointer.Targets.Contains(target.Value))
        {
            return;
        }

        if (!pinpointer.CanTargetMultiple)
        {
            pinpointer.Targets.Clear();
        }

        if (TerminatingOrDeleted(target.Value))
        {
            TrySetArrowAngle(uid, Angle.Zero, pinpointer);
            return;
        }

        pinpointer.Targets.Add(target.Value);

        if (pinpointer.UpdateTargetName)
            pinpointer.TargetName = Identity.Name(target.Value, EntityManager);
        // WD EDIT START - UpdateDirectionToTarget is triggered when updating, no need to run it again
        // if (pinpointer.IsActive)
        //    UpdateDirectionToTarget(uid, pinpointer);
        // WD EDIT END
    }

    /// <summary>
    /// Goob edit: sets a list of targets for a pinpointer.
    /// </summary>
    public virtual void SetTargets(EntityUid uid, List<EntityUid> targets, PinpointerComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        if (!pinpointer.CanTargetMultiple)
        {
            return; // No.
        }

        var targetsList = targets.Where(Exists).ToList();

        pinpointer.Targets = targetsList;

        // WD EDIT START - UpdateDirectionToTarget is triggered when updating, no need to run it again
        // if (pinpointer.IsActive)
        //    UpdateDirectionToTarget(uid, pinpointer);
        // WD EDIT END
    }

    /// <summary>
    ///     Update direction from pinpointer to selected target (if it was set)
    /// </summary>
    protected virtual void UpdateDirectionToTarget(EntityUid uid, PinpointerComponent? pinpointer = null)
    {

    }

    private void OnExamined(EntityUid uid, PinpointerComponent component, ExaminedEvent args)
    {
        if (!component.CanExamine || !args.IsInDetailsRange || component.TargetName == null) // WD EDIT
            return;

        args.PushMarkup(Loc.GetString("examine-pinpointer-linked", ("target", component.TargetName)));
    }

    /// <summary>
    ///     Manually set distance from pinpointer to target
    /// </summary>
    public void SetDistance(EntityUid uid, Distance distance, PinpointerComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        if (distance == pinpointer.DistanceToTarget)
            return;

        pinpointer.DistanceToTarget = distance;
        Dirty(uid, pinpointer);
    }

    /// <summary>
    ///     Try to manually set pinpointer arrow direction.
    ///     If difference between current angle and new angle is smaller than
    ///     pinpointer precision, new value will be ignored and it will return false.
    /// </summary>
    public bool TrySetArrowAngle(EntityUid uid, Angle arrowAngle, PinpointerComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return false;

        if (pinpointer.ArrowAngle.EqualsApprox(arrowAngle, pinpointer.Precision))
            return false;

        pinpointer.ArrowAngle = arrowAngle;
        Dirty(uid, pinpointer);

        return true;
    }

    /// <summary>
    ///     Activate/deactivate pinpointer screen. If it has target it will start tracking it.
    /// </summary>
    public void SetActive(EntityUid uid, bool isActive, PinpointerComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;
        if (isActive == pinpointer.IsActive)
            return;

        pinpointer.IsActive = isActive;
        Dirty(uid, pinpointer);
    }


    /// <summary>
    ///     Toggle Pinpointer screen. If it has target it will start tracking it.
    /// </summary>
    /// <returns>True if pinpointer was activated, false otherwise</returns>
    public virtual bool TogglePinpointer(EntityUid uid, PinpointerComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return false;

        var isActive = !pinpointer.IsActive;
        SetActive(uid, isActive, pinpointer);
        return isActive;
    }

    private void OnEmagged(EntityUid uid, PinpointerComponent component, ref GotEmaggedEvent args)
    {
        // WD EDIT START
        if (!component.CanEmag)
            return;
        // WD EDIT END

        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        args.Handled = true;

        if (component.CanRetarget)
        {
            component.RetargetingWhitelist = null; // Can target anything
            return;
        }

        component.CanRetarget = true;
    }
}
