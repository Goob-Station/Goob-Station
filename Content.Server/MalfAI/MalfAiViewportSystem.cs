// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Server.Station.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.MalfAI;

/// <summary>
/// Server logic for the Station AI / Malf AI "Viewport" ability:
/// - Handles world-targeted placement of the viewport area, respecting cooldown and station grid constraints.
/// - Streams viewport area PVS to the AI player using ViewSubscriberSystem (without hijacking session attached entity).
/// - Toggles the viewport UI on the AI client.
/// </summary>
public sealed class MalfAiViewportSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscriber = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiHeldComponent, MalfAiSetViewportActionEvent>(OnSetViewport);
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiOpenViewportActionEvent>(OnToggleViewport);
        SubscribeLocalEvent<MalfAiViewportComponent, ComponentShutdown>(OnViewportShutdown);
        SubscribeLocalEvent<StationAiHeldComponent, ActionPurchaseCompanionEvent>(OnActionPurchaseCompanion);
        SubscribeNetworkEvent<MalfAiViewportClosedMessage>(OnViewportClosed);
    }

    private void CleanupAnchor(EntityUid uid, MalfAiViewportComponent comp)
    {
        if (comp.ViewportAnchor != null && EntityManager.EntityExists(comp.ViewportAnchor.Value))
        {
            if (TryComp<ActorComponent>(uid, out var playerActor) && playerActor.PlayerSession != null)
            {
                _viewSubscriber.RemoveViewSubscriber(comp.ViewportAnchor.Value, playerActor.PlayerSession);
            }
            EntityManager.DeleteEntity(comp.ViewportAnchor.Value);
            comp.ViewportAnchor = null;
        }
    }

    private void OnViewportShutdown(EntityUid uid, MalfAiViewportComponent comp, ComponentShutdown args)
    {
        CleanupAnchor(uid, comp);

        if (TryComp<ActorComponent>(uid, out var actor) && actor.PlayerSession != null)
        {
            RaiseNetworkEvent(new MalfAiViewportCloseEvent(), actor.PlayerSession);
        }
    }

    private void OnViewportClosed(MalfAiViewportClosedMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is { } ai && TryComp<MalfAiViewportComponent>(ai, out var comp))
        {
            comp.IsWindowOpen = false;
        }
    }

    private void OnSetViewport(EntityUid uid, StationAiHeldComponent ai, ref MalfAiSetViewportActionEvent args)
    {
        if (args.Handled)
            return;

        var comp = EnsureComp<MalfAiViewportComponent>(uid);

        var now = _timing.CurTime;
        if (now < comp.NextSetAllowedAt)
        {
            _popup.PopupEntity(Loc.GetString("station-ai-viewport-cooldown"), uid, uid);
            args.Handled = true;
            return;
        }

        // Convert target to map coordinates.
        var target = _transform.ToMapCoordinates(args.Target);

        // Enforce grid constraint: target must be on a grid.
        if (!_map.TryFindGridAt(target, out var targetGridUid, out _))
        {
            _popup.PopupEntity(Loc.GetString("station-ai-viewport-out-of-range"), uid, uid);
            args.Handled = true;
            return;
        }

        // Validate station: target grid should belong to the same station if both belong to stations.
        var aiStation = _station.GetOwningStation(uid);
        var targetStation = _station.GetOwningStation(targetGridUid);
        if (aiStation != null && targetStation != null && aiStation != targetStation)
        {
            _popup.PopupEntity(Loc.GetString("station-ai-viewport-out-of-range"), uid, uid);
            args.Handled = true;
            return;
        }

        // Clean up any existing anchor entity.
        CleanupAnchor(uid, comp);

        // Convert map coordinates to local grid coordinates so the anchor entity is childed to the grid and rotates with it.
        var gridCoords = _transform.ToCoordinates(targetGridUid, target);

        // Spawn anchor entity parented to the grid.
        var anchor = EntityManager.SpawnEntity("StationAiViewportAnchor", gridCoords);
        comp.ViewportAnchor = anchor;

        var eye = EnsureComp<EyeComponent>(anchor);
        _eye.SetDrawFov(anchor, false, eye);
        _eye.SetDrawLight(anchor, true, eye);

        EnsureComp<CameraUpgradeOmitterComponent>(anchor);

        // Register the player's session as a view subscriber to the anchor entity so PVS streams the area.
        // NOTE: We MUST NEVER call SetAttachedEntity, as that disconnects the player from their AI body!
        if (TryComp<ActorComponent>(uid, out var playerActor) && playerActor.PlayerSession != null)
        {
            _viewSubscriber.AddViewSubscriber(anchor, playerActor.PlayerSession);
        }

        // Record chosen coordinates.
        comp.Selected = target;
        comp.NextSetAllowedAt = now + comp.SetCooldown;
        comp.IsWindowOpen = true;

        // Immediately open the viewport window for the AI client at the new position.
        if (TryComp<ActorComponent>(uid, out var actor) && actor.PlayerSession != null)
        {
            float rotation = (float)_transform.GetWorldRotation(targetGridUid).Theta;
            var ev = new MalfAiViewportOpenEvent(
                target.MapId,
                target.Position,
                comp.WindowSize,
                comp.Title,
                rotation,
                comp.ZoomLevel,
                GetNetEntity(comp.ViewportAnchor));
            RaiseNetworkEvent(ev, actor.PlayerSession);
        }

        args.Handled = true;
    }

    private void OnToggleViewport(EntityUid uid, StationAiHeldComponent ai, ref MalfAiOpenViewportActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MalfAiViewportComponent>(uid, out var comp) || comp.Selected is null || comp.ViewportAnchor is null || !EntityManager.EntityExists(comp.ViewportAnchor.Value))
        {
            _popup.PopupEntity(Loc.GetString("station-ai-viewport-not-placed"), uid, uid);
            args.Handled = true;
            return;
        }

        if (!TryComp<ActorComponent>(uid, out var actor) || actor.PlayerSession == null)
        {
            args.Handled = true;
            return;
        }

        // Toggle the viewport window state
        if (comp.IsWindowOpen)
        {
            // Close the window
            RaiseNetworkEvent(new MalfAiViewportCloseEvent(), actor.PlayerSession);
            comp.IsWindowOpen = false;
        }
        else
        {
            // Open the window
            var selected = comp.Selected.Value;
            var size = comp.WindowSize;
            var title = comp.Title;
            float rotation = 0f;
            if (_map.TryFindGridAt(selected, out var gridUid, out _))
            {
                rotation = (float)_transform.GetWorldRotation(gridUid).Theta;
            }

            // Ensure player is subscribed to the anchor's view
            _viewSubscriber.AddViewSubscriber(comp.ViewportAnchor.Value, actor.PlayerSession);

            var ev = new MalfAiViewportOpenEvent(
                selected.MapId,
                selected.Position,
                size,
                title,
                rotation,
                comp.ZoomLevel,
                GetNetEntity(comp.ViewportAnchor));
            RaiseNetworkEvent(ev, actor.PlayerSession);
            comp.IsWindowOpen = true;
        }

        args.Handled = true;
    }

    private void OnActionPurchaseCompanion(EntityUid buyer, StationAiHeldComponent comp, ref ActionPurchaseCompanionEvent args)
    {
        // Grant all companion actions specified in the event
        foreach (var actionId in args.CompanionActions)
        {
            if (!_mind.TryGetMind(buyer, out var mind, out _))
                _actions.AddAction(buyer, actionId);
            else
                _actionContainer.AddAction(mind, actionId);
        }
    }
}
