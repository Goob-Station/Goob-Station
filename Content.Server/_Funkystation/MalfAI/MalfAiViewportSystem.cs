// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Shared._Funkystation.Actions.Events;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Server logic for the Malf AI "Viewport" upgrade:
/// - Handles world-targeted setting of the viewport area, respecting a cooldown.
/// - Opens the viewport UI on the AI client after selection.
/// </summary>
public sealed class MalfAiViewportSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiViewportComponent, MalfAiSetViewportActionEvent>(OnSetViewport);
        SubscribeLocalEvent<MalfAiViewportComponent, MalfAiOpenViewportActionEvent>(OnToggleViewport);
        SubscribeLocalEvent<MalfAiViewportComponent, ComponentShutdown>(OnShutdown);
        SubscribeNetworkEvent<MalfAiViewportWindowClosedEvent>(OnClientWindowClosed);
        SubscribeLocalEvent<ActionPurchaseCompanionEvent>(OnActionPurchaseCompanion);
    }

    private void OnActionPurchaseCompanion(ActionPurchaseCompanionEvent args)
    {
        var buyer = GetEntity(args.Buyer);

        // Grant all companion actions specified in the event (e.g. Open Viewport alongside Set Viewport)
        foreach (var actionId in args.CompanionActions)
        {
            _actions.AddAction(buyer, actionId);
        }
    }

    private void OnSetViewport(Entity<MalfAiViewportComponent> ent, ref MalfAiSetViewportActionEvent args)
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;

        var now = _timing.CurTime;
        if (now < comp.NextSetTime)
        {
            // Still on cooldown; ignore the request.
            args.Handled = true;
            return;
        }

        // Convert target to map coordinates.
        var target = _xforms.ToMapCoordinates(args.Target);

        // Enforce same-grid constraint: target must be on the same grid as the AI.
        var aiCoords = _xforms.GetMapCoordinates(ent.Owner);
        if (!_map.TryFindGridAt(aiCoords, out var aiGridUid, out _)
            || !_map.TryFindGridAt(target, out var targetGridUid, out _)
            || aiGridUid != targetGridUid)
        {
            args.Handled = true;
            return;
        }

        // Clean up any existing anchor entity.
        if (comp.ViewportAnchor != null && Exists(comp.ViewportAnchor.Value))
        {
            Del(comp.ViewportAnchor.Value);
            comp.ViewportAnchor = null;
        }

        // Invisible anchor at the target; the client builds its own eye from the
        // rotation sent below so the viewport stays aligned with grid north.
        var anchor = Spawn(null, target);
        var gridRotation = (float) _xforms.GetWorldRotation(aiGridUid).Theta;
        _xforms.AnchorEntity(anchor, Transform(anchor));

        comp.ViewportAnchor = anchor;
        comp.Selected = target;
        comp.NextSetTime = now + comp.SetCooldown;

        // Immediately open the viewport window for the AI client at the new position.
        if (TryComp<ActorComponent>(ent.Owner, out var actor))
        {
            var ev = new MalfAiViewportOpenEvent(
                target.MapId,
                target.Position,
                comp.WindowSize,
                Loc.GetString("malfai-viewport-title"),
                gridRotation,
                comp.ZoomLevel,
                GetNetEntity(anchor));
            RaiseNetworkEvent(ev, actor.PlayerSession);
            comp.IsWindowOpen = true;
        }

        args.Handled = true;
    }

    private void OnToggleViewport(Entity<MalfAiViewportComponent> ent, ref MalfAiOpenViewportActionEvent args)
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;

        if (comp.Selected is null)
        {
            // No configured viewport; ignore.
            args.Handled = true;
            return;
        }

        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
        {
            args.Handled = true;
            return;
        }

        if (comp.IsWindowOpen)
        {
            RaiseNetworkEvent(new MalfAiViewportCloseEvent(), actor.PlayerSession);
            comp.IsWindowOpen = false;
        }
        else
        {
            var selected = comp.Selected.Value;
            var rotation = 0f;
            var aiCoords = _xforms.GetMapCoordinates(ent.Owner);
            if (_map.TryFindGridAt(aiCoords, out var aiGridUid, out _))
                rotation = (float) _xforms.GetWorldRotation(aiGridUid).Theta;

            var ev = new MalfAiViewportOpenEvent(
                selected.MapId,
                selected.Position,
                comp.WindowSize,
                Loc.GetString("malfai-viewport-title"),
                rotation,
                comp.ZoomLevel,
                GetNetEntity(comp.ViewportAnchor));
            RaiseNetworkEvent(ev, actor.PlayerSession);
            comp.IsWindowOpen = true;
        }

        args.Handled = true;
    }

    private void OnClientWindowClosed(MalfAiViewportWindowClosedEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid)
            return;

        if (TryComp<MalfAiViewportComponent>(uid, out var viewport))
            viewport.IsWindowOpen = false;
    }

    private void OnShutdown(Entity<MalfAiViewportComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ViewportAnchor is { } anchor)
            QueueDel(anchor);
    }
}
