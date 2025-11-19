// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Player;
using Robust.Server.Player;
using Content.Shared.Actions.Components;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Server._Funkystation.MalfAI.Components;
using Content.Shared._Funkystation.Actions.Events;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Server logic for the Malf AI "Viewport" upgrade:
/// - Handles world-targeted setting of the viewport area, respecting a cooldown.
/// - Opens the viewport UI on the AI client after selection.
/// </summary>
public sealed class MalfAiViewportSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        // Only the AI (holding the StationAiHeldComponent) can use these actions.
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiSetViewportActionEvent>(OnSetViewport);
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiOpenViewportActionEvent>(OnToggleViewport);
        SubscribeLocalEvent<StationAiHeldComponent, ComponentInit>(OnAiInit);
        SubscribeLocalEvent<ActionPurchaseCompanionEvent>(OnActionPurchaseCompanion);
    }

    private bool HasActionPrototype(EntityUid uid, string protoId)
    {
        if (!TryComp<ActionsComponent>(uid, out var actionsComp))
            return false;

        foreach (var action in actionsComp.Actions)
        {
            if (MetaData(action).EntityPrototype?.ID == protoId)
                return true;
        }

        return false;
    }

    private void OnAiInit(EntityUid uid, StationAiHeldComponent comp, ref ComponentInit args)
    {
        // Ensure both viewport actions are present if the place action is present
        if (HasActionPrototype(uid, "ActionMalfAiSetViewport") && !HasActionPrototype(uid, "ActionMalfAiOpenViewport"))
        {
            _actions.AddAction(uid, "ActionMalfAiOpenViewport");
        }
    }

    private void OnSetViewport(EntityUid uid, StationAiHeldComponent ai, ref MalfAiSetViewportActionEvent args)
    {
        if (args.Handled)
            return;

        // Ensure component exists.
        var comp = EnsureComp<MalfAiViewportComponent>(uid);

        var now = _timing.CurTime;
        if (now < comp.NextSetAllowedAt)
        {
            // Still on cooldown; ignore the request.
            args.Handled = true;
            return;
        }

        // Convert target to map coordinates.
        var target = _xform.ToMapCoordinates(args.Target);

        // Enforce same-grid constraint: target must be on the same grid as the AI.
        var aiCoords = _xform.GetMapCoordinates(uid);
        if (!_map.TryFindGridAt(aiCoords, out var aiGridUid, out _)
            || !_map.TryFindGridAt(target, out var targetGridUid, out _)
            || aiGridUid != targetGridUid)
        {
            args.Handled = true;
            return;
        }

        // Clean up any existing anchor entity
        if (comp.ViewportAnchor != null && Exists(comp.ViewportAnchor.Value))
        {
            // Properly detach player session if attached
            if (TryComp<ActorComponent>(comp.ViewportAnchor.Value, out var oldActor) &&
                oldActor.PlayerSession != null)
            {
                _playerManager.SetAttachedEntity(oldActor.PlayerSession, null);
            }
            EntityManager.DeleteEntity(comp.ViewportAnchor.Value);
            comp.ViewportAnchor = null;
        }

        // Create invisible anchor entity at the target location with eye component for camera functionality
        comp.ViewportAnchor = EntityManager.SpawnEntity(null, target);

        // Add actor component to make it a proper client eye
        var actorComp = EnsureComp<ActorComponent>(comp.ViewportAnchor.Value);

        // Make the anchor act as an active renderer by attaching the player's session to it
        // This ensures the client receives updates from the anchor's perspective
        if (TryComp<ActorComponent>(uid, out var playerActor) && playerActor.PlayerSession != null)
        {
            // Use player manager to properly attach the session to the anchor entity
            _playerManager.SetAttachedEntity(playerActor.PlayerSession, comp.ViewportAnchor.Value);
        }

        // Add the camera upgrade omitter component to prevent this anchor from being considered for camera upgrades
        EnsureComp<CameraUpgradeOmitterComponent>(comp.ViewportAnchor.Value);

        // Apply grid north rotation to the anchor entity's transform so viewport faces grid north
        float anchorRotation = 0f;
        if (_map.TryFindGridAt(aiCoords, out var gridForRotation, out _))
        {
            anchorRotation = (float) _xform.GetWorldRotation(gridForRotation).Theta;
        }
        // Set the anchor entity's local rotation to counter the grid rotation

        var anchorXform = Transform(comp.ViewportAnchor.Value);

        anchorXform.LocalRotation = -anchorRotation;

        // Anchor the entity to the grid so it moves with the grid
        _xform.AnchorEntity(comp.ViewportAnchor.Value, anchorXform);

        // Record chosen coordinates.
        comp.Selected = target;

        // Set next allowed time.
        comp.NextSetAllowedAt = now + comp.SetCooldown;

        // Immediately open the viewport window for the AI client at the new position.
        if (TryComp<ActorComponent>(uid, out var actor) && actor.PlayerSession != null)
        {
            float rotation = 0f;
            if (_map.TryFindGridAt(aiCoords, out var aiGridUid2, out _))
            {
                rotation = (float) _xform.GetWorldRotation(aiGridUid2).Theta;
            }
            var ev = new MalfAiViewportOpenEvent(target.MapId, target.Position, comp.WindowSize, comp.Title, rotation, comp.ZoomLevel, GetNetEntity(comp.ViewportAnchor));
            RaiseNetworkEvent(ev, actor.PlayerSession);
        }

        args.Handled = true;
    }

    private void OnToggleViewport(EntityUid uid, StationAiHeldComponent ai, ref MalfAiOpenViewportActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MalfAiViewportComponent>(uid, out var comp) || comp.Selected is null)
        {
            // No configured viewport; ignore.
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
            var aiCoords = _xform.GetMapCoordinates(uid);
            if (_map.TryFindGridAt(aiCoords, out var aiGridUid, out _))
            {
                rotation = (float) _xform.GetWorldRotation(aiGridUid).Theta;
            }
            var ev = new MalfAiViewportOpenEvent(selected.MapId, selected.Position, size, title, rotation, comp.ZoomLevel, GetNetEntity(comp.ViewportAnchor));
            RaiseNetworkEvent(ev, actor.PlayerSession);
            comp.IsWindowOpen = true;
        }

        args.Handled = true;
    }

    private void OnActionPurchaseCompanion(ActionPurchaseCompanionEvent args)
    {
        var buyer = GetEntity(args.Buyer);

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
