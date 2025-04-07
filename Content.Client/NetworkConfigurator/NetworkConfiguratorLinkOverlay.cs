// SPDX-FileCopyrightText: 2022 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Client.NetworkConfigurator.Systems;
using Content.Shared.DeviceNetwork.Components;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Client.NetworkConfigurator;

public sealed class NetworkConfiguratorLinkOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private readonly DeviceListSystem _deviceListSystem;
    private readonly SharedTransformSystem _transformSystem;

    public Dictionary<EntityUid, Color> Colors = new();
    public EntityUid? Action;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public NetworkConfiguratorLinkOverlay()
    {
        IoCManager.InjectDependencies(this);

        _deviceListSystem = _entityManager.System<DeviceListSystem>();
        _transformSystem = _entityManager.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var query = _entityManager.EntityQueryEnumerator<NetworkConfiguratorActiveLinkOverlayComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_entityManager.Deleted(uid) || !_entityManager.TryGetComponent(uid, out DeviceListComponent? deviceList))
            {
                _entityManager.RemoveComponentDeferred<NetworkConfiguratorActiveLinkOverlayComponent>(uid);
                continue;
            }

            if (!Colors.TryGetValue(uid, out var color))
            {
                color = new Color(
                    _random.Next(0, 255),
                    _random.Next(0, 255),
                    _random.Next(0, 255));
                Colors.Add(uid, color);
            }

            var sourceTransform = _entityManager.GetComponent<TransformComponent>(uid);
            if (sourceTransform.MapID == MapId.Nullspace)
            {
                // Can happen if the item is outside the client's view. In that case,
                // we don't have a sensible transform to draw, so we need to skip it.
                continue;
            }

            foreach (var device in _deviceListSystem.GetAllDevices(uid, deviceList))
            {
                if (_entityManager.Deleted(device))
                {
                    continue;
                }

                var linkTransform = _entityManager.GetComponent<TransformComponent>(device);
                if (linkTransform.MapID == MapId.Nullspace)
                {
                    continue;
                }

                args.WorldHandle.DrawLine(_transformSystem.GetWorldPosition(sourceTransform), _transformSystem.GetWorldPosition(linkTransform), Colors[uid]);
            }
        }
    }
}