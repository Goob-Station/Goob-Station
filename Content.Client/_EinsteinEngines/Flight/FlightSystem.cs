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
