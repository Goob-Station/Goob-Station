// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Content.Shared.MalfAI;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store.Components;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Store;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Client.UserInterface.Systems.Alerts.Widgets; using Robust.Shared.Localization;

namespace Content.Client.MalfAI;

/// <summary>
/// Displays a small HUD on the right side of the screen for Malf AI showing current CPU.
/// Placed directly under the chat window by mirroring Chat's bottom margin.
/// </summary>
public sealed class MalfAiCpuHudSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    private const bool DisabledLegacyHud = false;

    private PanelContainer? _panel;
    private Label? _label;

    private ResizableChatBox? _chat;
    private bool _chatSubscribed;
    private Content.Client.UserInterface.Systems.Alerts.Widgets.AlertsUI? _alerts;

    // Update interval to refresh displayed CPU while visible
    private const float UpdateInterval = 0.25f;
    private float _accum;

    // Colors styled to match Malf UI
    private static readonly Color MalfGreen = new(0f, 1f, 0f);

    public override void Initialize()
    {
        base.Initialize();
        if (DisabledLegacyHud)
            return;
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        if (_chat != null && _chatSubscribed)
        {
            _chat.OnResized -= OnChatResized;
            _chatSubscribed = false;
        }
        RemoveHud();
    }

    private void OnPlayerAttached(LocalPlayerAttachedEvent ev)
    {
        EnsureHud();
        TryResolveChat();
        UpdateVisibilityAndText(force: true);
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent ev)
    {
        UpdateVisibilityAndText(force: true);
    }

    public override void FrameUpdate(float frameTime)
    {
        if (DisabledLegacyHud)
            return;
        base.FrameUpdate(frameTime);
        _accum += frameTime;
        if (_accum < UpdateInterval)
            return;
        _accum = 0f;
        TryResolveChat();
        UpdateVisibilityAndText();
    }

    private void EnsureHud()
    {
        if (_panel != null)
            return;

        // Create UI
        _panel = new PanelContainer
        {
            Visible = false
        };

        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.Black,
            BorderColor = MalfGreen,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 6;
        style.ContentMarginTopOverride = 4;
        style.ContentMarginRightOverride = 6;
        style.ContentMarginBottomOverride = 4;
        _panel.PanelOverride = style;

        _label = new Label
        {
            Text = "CPU: 0",
            Modulate = MalfGreen
        };
        _panel.AddChild(_label);

        // Anchor to top-right, margin will be dynamically adjusted to be under Chat
        LayoutContainer.SetAnchorPreset(_panel, LayoutContainer.LayoutPreset.TopRight);

        _ui.StateRoot.AddChild(_panel);
        UpdateMargins();
    }

    private void RemoveHud()
    {
        if (_panel == null)
            return;
        _ui.StateRoot.RemoveChild(_panel);
        _panel.Dispose();
        _panel = null;
        _label = null;
    }

    private void TryResolveChat()
    {
        if (_chat == null)
            _chat = FindChatControl(_ui.StateRoot);

        if (_chat != null && !_chatSubscribed)
        {
            _chat.OnResized += OnChatResized;
            _chatSubscribed = true;
        }

        // Make sure our HUD lives in the same parent as Chat to keep coordinate space consistent.
        EnsurePanelParent();

        // Also resolve Alerts while we’re here.
        if (_alerts == null)
            _alerts = FindAlertsControl(_ui.StateRoot);

        UpdateMargins();
    }

    private void OnChatResized()
    {
        UpdateMargins();
    }

    private void EnsurePanelParent()
    {
        if (_panel == null || _chat == null)
            return;

        var chatParent = _chat.Parent;
        if (chatParent == null)
            return;

        if (!ReferenceEquals(_panel.Parent, chatParent))
        {
            // Move HUD panel under the same parent as Chat so anchor/margins operate in the same space.
            _panel.Parent?.RemoveChild(_panel);
            chatParent.AddChild(_panel);
            LayoutContainer.SetAnchorPreset(_panel, LayoutContainer.LayoutPreset.TopRight);
        }
    }

    private void UpdateMargins()
    {
        if (_panel == null)
            return;

        float chatBottom = 10f;
        if (_chat != null)
        {
            // Mirror DefaultGameScreen behavior: place directly under chat
            chatBottom = _chat.GetValue<float>(LayoutContainer.MarginBottomProperty);
        }

        // Position HUD under Chat
        LayoutContainer.SetMarginTop(_panel, chatBottom);
        // Keep right margin default (10) like other HUDs
        LayoutContainer.SetMarginRight(_panel, 10f);

        // Adjust Alerts to sit below the HUD when visible to avoid overlap.
        if (_alerts != null)
        {
            var extra = (_panel.Visible ? _panel.Size.Y + 8f : 0f);
            var alertsTop = chatBottom + extra;
            LayoutContainer.SetMarginTop(_alerts, alertsTop);
        }
    }

    private ResizableChatBox? FindChatControl(Control root)
    {
        foreach (var child in root.Children)
        {
            if (child is ResizableChatBox chat)
                return chat;

            if (child.ChildCount > 0)
            {
                var found = FindChatControl(child);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private AlertsUI? FindAlertsControl(Control root)
    {
        foreach (var child in root.Children)
        {
            if (child is AlertsUI alerts)
                return alerts;

            if (child.ChildCount > 0)
            {
                var found = FindAlertsControl(child);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private EntityUid? ResolveMalfAiEntity(EntityUid local)
    {
        // Prefer local if it already has a Store (covers cases where the eye holds store).
        if (TryComp<StoreComponent>(local, out _))
            return local;

        // First try: find any entity that has both the MalfAiMarker and a Store.
        var query = AllEntityQuery<MalfAiMarkerComponent, StoreComponent>();
        EntityUid? candidate = null;
        while (query.MoveNext(out var uid, out _, out _))
        {
            // Prefer ones owned by the local player if ActorComponent matches (best-effort).
            // If we can access ActorComponent on the same uid, use that; otherwise just take the first.
            candidate ??= uid;
        }

        return candidate;
    }

    private void UpdateVisibilityAndText(bool force = false)
    {
        EnsureHud();
        if (_panel == null)
            return;

        var localOpt = _players.LocalEntity;
        var shouldShow = localOpt.HasValue && (HasComp<StationAiHeldComponent>(localOpt.Value) || HasComp<MalfAiMarkerComponent>(localOpt.Value));

        if (!shouldShow)
        {
            if (_panel.Visible)
                _panel.Visible = false;
            return;
        }

        if (!_panel.Visible)
            _panel.Visible = true;

        // Update text from the resolved Malf AI entity (may differ from local AI eye)
        if (_label != null && localOpt.HasValue)
        {
            var source = ResolveMalfAiEntity(localOpt.Value);
            if (source.HasValue && TryComp<StoreComponent>(source.Value, out var store))
            {
                ProtoId<CurrencyPrototype> cpu = "CPU";
                if (store.Balance.TryGetValue(cpu, out FixedPoint2 amount))
                    _label.Text = $"{Loc.GetString("CPU")}: {amount}";
                else
                    _label.Text = $"{Loc.GetString("CPU")}: 0";
            }
            else
            {
                // Fallback: try local directly
                if (TryComp<StoreComponent>(localOpt.Value, out var localStore))
                {
                    ProtoId<CurrencyPrototype> cpu = "CPU";
                    if (localStore.Balance.TryGetValue(cpu, out FixedPoint2 amount))
                        _label.Text = $"{Loc.GetString("CPU")}: {amount}";
                    else
                        _label.Text = $"{Loc.GetString("CPU")}: 0";
                }
                else
                {
                    _label.Text = $"{Loc.GetString("CPU")}: 0";
                }
            }
        }
    }
}
