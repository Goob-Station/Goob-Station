// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kyle Tyo <36606155+VerinSenpai@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Misc;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.Weapons.Misc;

public sealed class TetherGunSystem : SharedTetherGunSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TetheredComponent, ComponentStartup>(OnTetheredStartup);
        SubscribeLocalEvent<TetheredComponent, ComponentShutdown>(OnTetheredShutdown);
        SubscribeLocalEvent<TetherGunComponent, AfterAutoHandleStateEvent>(OnAfterState);
        SubscribeLocalEvent<ForceGunComponent, AfterAutoHandleStateEvent>(OnAfterState);
        _overlay.AddOverlay(new TetherGunOverlay(EntityManager));
    }

    private void OnAfterState(EntityUid uid, BaseForceGunComponent component, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(component.Tethered, out var sprite))
            return;

        sprite.Color = component.LineColor;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<TetherGunOverlay>();
    }

    protected override bool CanTether(EntityUid uid, BaseForceGunComponent component, EntityUid target, EntityUid? user)
    {
        // Need powercells predicted sadly :<
        return false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var player = _player.LocalEntity;

        if (player == null ||
            !TryGetTetherGun(player.Value, out var gunUid, out var gun) ||
            gun.TetherEntity == null)
        {
            return;
        }

        var mousePos = _input.MouseScreenPosition;
        var mouseWorldPos = _eyeManager.PixelToMap(mousePos);

        if (mouseWorldPos.MapId == MapId.Nullspace)
            return;

        EntityCoordinates coords;

        if (_mapManager.TryFindGridAt(mouseWorldPos, out var gridUid, out _))
        {
            coords = TransformSystem.ToCoordinates(gridUid, mouseWorldPos);
        }
        else
        {
            coords = TransformSystem.ToCoordinates(_mapSystem.GetMap(mouseWorldPos.MapId), mouseWorldPos);
        }

        const float BufferDistance = 0.1f;

        if (TryComp(gun.TetherEntity, out TransformComponent? tetherXform) &&
            tetherXform.Coordinates.TryDistance(EntityManager, TransformSystem, coords, out var distance) &&
            distance < BufferDistance)
        {
            return;
        }

        RaisePredictiveEvent(new RequestTetherMoveEvent()
        {
            Coordinates = GetNetCoordinates(coords)
        });
    }

    private void OnTetheredStartup(EntityUid uid, TetheredComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
        {
            return;
        }

        if (TryComp<ForceGunComponent>(component.Tetherer, out var force))
        {
            sprite.Color = force.LineColor;
        }
        else if (TryComp<TetherGunComponent>(component.Tetherer, out var tether))
        {
            sprite.Color = tether.LineColor;
        }
    }

    private void OnTetheredShutdown(EntityUid uid, TetheredComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.Color = Color.White;
    }
}