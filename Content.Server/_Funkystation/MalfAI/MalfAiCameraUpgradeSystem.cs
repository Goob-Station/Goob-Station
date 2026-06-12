// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI camera-upgrade purchase and keeps the Effective flag
/// in sync with whether the AI is held (core or shunted APC).
/// </summary>
public sealed class MalfAiCameraUpgradeSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Held status changes (enable when entering a holder if desired; disable on leaving)
        SubscribeLocalEvent<StationAiHeldComponent, ComponentStartup>(OnHeldStartup);
        SubscribeLocalEvent<StationAiHeldComponent, ComponentShutdown>(OnHeldShutdown);

        // Grant-on-purchase event
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiCameraUpgradeUnlockedEvent>(OnCameraUpgradeUnlocked);

        // Toggle action
        SubscribeLocalEvent<MalfAiCameraUpgradeComponent, MalfAiToggleCameraUpgradeActionEvent>(OnToggle);
    }

    private void OnToggle(Entity<MalfAiCameraUpgradeComponent> ent, ref MalfAiToggleCameraUpgradeActionEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.EnabledDesired = !ent.Comp.EnabledDesired;
        ent.Comp.EnabledEffective = ent.Comp.EnabledDesired && HasComp<StationAiHeldComponent>(ent.Owner);
        Dirty(ent);

        var key = ent.Comp.EnabledDesired ? "malfai-camera-upgrade-enabled" : "malfai-camera-upgrade-disabled";
        _popup.PopupEntity(Loc.GetString(key), ent.Owner, ent.Owner);
        args.Handled = true;
    }

    private void OnCameraUpgradeUnlocked(Entity<MalfAiMarkerComponent> ent, ref MalfAiCameraUpgradeUnlockedEvent args)
    {
        _actions.AddAction(ent.Owner, "ActionMalfAiToggleCameraUpgrade");

        var comp = EnsureComp<MalfAiCameraUpgradeComponent>(ent.Owner);
        comp.EnabledDesired = true;

        // Effective only while the AI is held (StationAiHeldComponent present).
        comp.EnabledEffective = HasComp<StationAiHeldComponent>(ent.Owner);
        Dirty(ent.Owner, comp);
    }

    private void OnHeldStartup(Entity<StationAiHeldComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<MalfAiCameraUpgradeComponent>(ent.Owner, out var comp))
            return;

        if (comp.EnabledEffective != comp.EnabledDesired)
        {
            comp.EnabledEffective = comp.EnabledDesired;
            Dirty(ent.Owner, comp);
        }
    }

    private void OnHeldShutdown(Entity<StationAiHeldComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<MalfAiCameraUpgradeComponent>(ent.Owner, out var comp))
            return;

        if (comp.EnabledEffective)
        {
            comp.EnabledEffective = false;
            Dirty(ent.Owner, comp);
        }
    }
}
