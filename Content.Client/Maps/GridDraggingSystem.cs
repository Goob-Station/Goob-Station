// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Content.Shared.Maps;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Client.Maps;

/// <inheritdoc />
public sealed class GridDraggingSystem : SharedGridDraggingSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly InputSystem _inputSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public bool Enabled { get; set; }

    private EntityUid? _dragging;
    private Vector2 _localPosition;
    private MapCoordinates? _lastMousePosition;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GridDragToggleMessage>(OnToggleMessage);
    }

    private void OnToggleMessage(GridDragToggleMessage ev)
    {
        if (Enabled == ev.Enabled)
            return;

        Enabled = ev.Enabled;

        if (!Enabled)
            StopDragging();
    }

    private void StartDragging(EntityUid grid, Vector2 localPosition)
    {
        _dragging = grid;
        _localPosition = localPosition;

        if (HasComp<PhysicsComponent>(grid))
        {
            RaiseNetworkEvent(new GridDragVelocityRequest()
            {
                Grid = GetNetEntity(grid),
                LinearVelocity = Vector2.Zero
            });
        }
    }

    private void StopDragging()
    {
        if (_dragging == null) return;

        if (_lastMousePosition != null && TryComp(_dragging.Value, out TransformComponent? xform) &&
            TryComp<PhysicsComponent>(_dragging.Value, out _) &&
            xform.MapID == _lastMousePosition.Value.MapId)
        {
            var tickTime = _gameTiming.TickPeriod;
            var distance = _lastMousePosition.Value.Position - _transformSystem.GetWorldPosition(xform);
            RaiseNetworkEvent(new GridDragVelocityRequest()
            {
                Grid = GetNetEntity(_dragging.Value),
                LinearVelocity = distance.LengthSquared() > 0f ? (distance / (float) tickTime.TotalSeconds) * 0.25f : Vector2.Zero,
            });
        }

        _dragging = null;
        _localPosition = Vector2.Zero;
        _lastMousePosition = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Enabled || !_gameTiming.IsFirstTimePredicted) return;

        var state = _inputSystem.CmdStates.GetState(EngineKeyFunctions.Use);

        if (state != BoundKeyState.Down)
        {
            StopDragging();
            return;
        }

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.PixelToMap(mouseScreenPos);

        if (_dragging == null)
        {
            if (!_mapManager.TryFindGridAt(mousePos, out var gridUid, out var grid))
                return;

            StartDragging(gridUid, Vector2.Transform(mousePos.Position, _transformSystem.GetInvWorldMatrix(gridUid)));
        }

        if (!TryComp(_dragging, out TransformComponent? xform))
        {
            StopDragging();
            return;
        }

        if (xform.MapID != mousePos.MapId)
        {
            StopDragging();
            return;
        }

        var localToWorld = Vector2.Transform(_localPosition, _transformSystem.GetWorldMatrix(xform));

        if (localToWorld.EqualsApprox(mousePos.Position, 0.01f)) return;

        var requestedGridOrigin = mousePos.Position - _transformSystem.GetWorldRotation(xform).RotateVec(_localPosition);
        _lastMousePosition = new MapCoordinates(requestedGridOrigin, mousePos.MapId);

        RaiseNetworkEvent(new GridDragRequestPosition()
        {
            Grid = GetNetEntity(_dragging.Value),
            WorldPosition = requestedGridOrigin,
        });
    }
}