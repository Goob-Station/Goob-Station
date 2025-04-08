// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <super.novalskiy_0135@inbox.ru>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Tools.Systems;
using Content.Shared.UserInterface;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Wires;

public abstract class SharedWiresSystem : EntitySystem
{
    [Dependency] protected readonly ISharedAdminLogManager AdminLogger = default!;
    [Dependency] private readonly ActivatableUISystem _activatableUI = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedToolSystem Tool = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WiresPanelComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WiresPanelComponent, WirePanelDoAfterEvent>(OnPanelDoAfter);
        SubscribeLocalEvent<WiresPanelComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<WiresPanelComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<ActivatableUIRequiresPanelComponent, ActivatableUIOpenAttemptEvent>(OnAttemptOpenActivatableUI);
        SubscribeLocalEvent<ActivatableUIRequiresPanelComponent, PanelChangedEvent>(OnActivatableUIPanelChanged);
    }

    private void OnStartup(Entity<WiresPanelComponent> ent, ref ComponentStartup args)
    {
        UpdateAppearance(ent, ent);
    }

    private void OnPanelDoAfter(EntityUid uid, WiresPanelComponent panel, WirePanelDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TogglePanel(uid, panel, !panel.Open, args.User))
            return;

        AdminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):user} screwed {ToPrettyString(uid):target}'s maintenance panel {(panel.Open ? "open" : "closed")}");

        var sound = panel.Open ? panel.ScrewdriverOpenSound : panel.ScrewdriverCloseSound;
        Audio.PlayPredicted(sound, uid, args.User);
        args.Handled = true;
    }

    private void OnInteractUsing(Entity<WiresPanelComponent> ent, ref InteractUsingEvent args)
    {
        if (!Tool.HasQuality(args.Used, ent.Comp.OpeningTool))
            return;

        if (!CanTogglePanel(ent, args.User))
            return;

        if (!Tool.UseTool(
                args.Used,
                args.User,
                ent,
                (float) ent.Comp.OpenDelay.TotalSeconds,
                ent.Comp.OpeningTool,
                new WirePanelDoAfterEvent()))
        {
            return;
        }

        AdminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(args.User):user} is screwing {ToPrettyString(ent):target}'s {(ent.Comp.Open ? "open" : "closed")} maintenance panel at {Transform(ent).Coordinates:targetlocation}");
        args.Handled = true;
    }

    private void OnExamine(EntityUid uid, WiresPanelComponent component, ExaminedEvent args)
    {
        using (args.PushGroup(nameof(WiresPanelComponent)))
        {
            if (!component.Open)
            {
                if (!string.IsNullOrEmpty(component.ExamineTextClosed))
                    args.PushMarkup(Loc.GetString(component.ExamineTextClosed));
            }
            else
            {
                if (!string.IsNullOrEmpty(component.ExamineTextOpen))
                    args.PushMarkup(Loc.GetString(component.ExamineTextOpen));

                if (TryComp<WiresPanelSecurityComponent>(uid, out var wiresPanelSecurity) &&
                    wiresPanelSecurity.Examine != null)
                {
                    args.PushMarkup(Loc.GetString(wiresPanelSecurity.Examine));
                }
            }
        }
    }

    public void ChangePanelVisibility(EntityUid uid, WiresPanelComponent component, bool visible)
    {
        component.Visible = visible;
        UpdateAppearance(uid, component);
        Dirty(uid, component);
    }

    protected void UpdateAppearance(EntityUid uid, WiresPanelComponent panel)
    {
        if (TryComp<AppearanceComponent>(uid, out var appearance))
            Appearance.SetData(uid, WiresVisuals.MaintenancePanelState, panel.Open && panel.Visible, appearance);
    }

    public bool TogglePanel(EntityUid uid, WiresPanelComponent component, bool open, EntityUid? user = null)
    {
        if (!CanTogglePanel((uid, component), user))
            return false;

        component.Open = open;
        UpdateAppearance(uid, component);
        Dirty(uid, component);

        var ev = new PanelChangedEvent(component.Open);
        RaiseLocalEvent(uid, ref ev);
        return true;
    }

    public bool CanTogglePanel(Entity<WiresPanelComponent> ent, EntityUid? user)
    {
        var attempt = new AttemptChangePanelEvent(ent.Comp.Open, user);
        RaiseLocalEvent(ent, ref attempt);
        return !attempt.Cancelled;
    }

    public bool IsPanelOpen(Entity<WiresPanelComponent?> entity, EntityUid? tool = null)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return true;

        if (tool != null)
        {
            var ev = new PanelOverrideEvent();
            RaiseLocalEvent(tool.Value, ref ev);

            if (ev.Allowed)
                return true;
        }

        // Listen, i don't know what the fuck this component does. it's stapled on shit for airlocks
        // but it looks like an almost direct duplication of WiresPanelComponent except with a shittier API.
        if (TryComp<WiresPanelSecurityComponent>(entity, out var wiresPanelSecurity) &&
            !wiresPanelSecurity.WiresAccessible)
            return false;

        return entity.Comp.Open;
    }

    private void OnAttemptOpenActivatableUI(EntityUid uid, ActivatableUIRequiresPanelComponent component, ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled || !TryComp<WiresPanelComponent>(uid, out var wires))
            return;

        if (component.RequireOpen != wires.Open)
            args.Cancel();
    }

    private void OnActivatableUIPanelChanged(EntityUid uid, ActivatableUIRequiresPanelComponent component, ref PanelChangedEvent args)
    {
        if (args.Open == component.RequireOpen)
            return;

        _activatableUI.CloseAll(uid);
    }
}

/// <summary>
/// Raised directed on a tool to try and override panel visibility.
/// </summary>
[ByRefEvent]
public record struct PanelOverrideEvent()
{
    public bool Allowed = true;
}