// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.GameObjects;
using Content.Shared._EinsteinEngines.Flight;
using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Client._EinsteinEngines.Flight.Components;

namespace Content.Client._EinsteinEngines.Flight;
public sealed class FlightSystem : SharedFlightSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleFlightVisualsEvent>(OnToggleFlightVisuals); // We need this crap because standingsys only raises shit on server lmao
        SubscribeLocalEvent<FlightComponent, FlightEvent>(OnFlight);
    }

    private void OnToggleFlightVisuals(ToggleFlightVisualsEvent args)
    {
        if (!TryGetEntity(args.Uid, out var uid)
            || !TryComp<FlightComponent>(uid, out var flight))
            return;

        HandleFlightToggle(uid.Value, flight, args.IsFlying, args.IsAnimated);
    }

    private void OnFlight(EntityUid uid, FlightComponent component, FlightEvent args) =>
        HandleFlightToggle(uid, component, args.IsFlying, component.IsAnimated);

    private void HandleFlightToggle(EntityUid uid,
        FlightComponent component,
        bool isFlying,
        bool isAnimated)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !isAnimated)
            return;

        int? targetLayer = null;
        if (component.IsLayerAnimated && component.Layer is not null)
        {
            targetLayer = GetAnimatedLayer(uid, component.Layer, sprite);
            if (targetLayer == null)
                return;
        }

        if (isFlying
            && isAnimated
            && component.AnimationKey != "default"
            && !HasComp<FlightVisualsComponent>(uid))
        {
            var comp = new FlightVisualsComponent
            {
                AnimateLayer = component.IsLayerAnimated,
                AnimationKey = component.AnimationKey,
                Multiplier = component.ShaderMultiplier,
                Offset = component.ShaderOffset,
                Speed = component.ShaderSpeed,
                TargetLayer = targetLayer,
            };
            AddComp(uid, comp);
        }
        if (!isFlying)
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