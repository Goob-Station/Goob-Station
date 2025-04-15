// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.GameObjects;
using Content.Shared._EinsteinEngines.Flight;
using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Client._EinsteinEngines.Flight.Components;

namespace Content.Client._EinsteinEngines.Flight;
public sealed class FlightSystem : SharedFlightSystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<FlightEvent>(OnFlight);

    }

    private void OnFlight(FlightEvent args)
    {
        var uid = GetEntity(args.Uid);
        if (!_entityManager.TryGetComponent(uid, out SpriteComponent? sprite)
            || !args.IsAnimated
            || !_entityManager.TryGetComponent(uid, out FlightComponent? flight))
            return;


        int? targetLayer = null;
        if (flight.IsLayerAnimated && flight.Layer is not null)
        {
            targetLayer = GetAnimatedLayer(uid, flight.Layer, sprite);
            if (targetLayer == null)
                return;
        }

        if (args.IsFlying && args.IsAnimated && flight.AnimationKey != "default")
        {
            var comp = new FlightVisualsComponent
            {
                AnimateLayer = flight.IsLayerAnimated,
                AnimationKey = flight.AnimationKey,
                Multiplier = flight.ShaderMultiplier,
                Offset = flight.ShaderOffset,
                Speed = flight.ShaderSpeed,
                TargetLayer = targetLayer,
            };
            AddComp(uid, comp);
        }
        if (!args.IsFlying)
            RemComp<FlightVisualsComponent>(uid);
    }

    public int? GetAnimatedLayer(EntityUid uid, string targetLayer, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref sprite))
            return null;

        int index = 0;
        foreach (var layer in sprite.AllLayers)
        {
            // This feels like absolute shitcode, isn't there a better way to check for it?
            if (layer.Rsi?.Path.ToString() == targetLayer)
                return index;
            index++;
        }
        return null;
    }
}