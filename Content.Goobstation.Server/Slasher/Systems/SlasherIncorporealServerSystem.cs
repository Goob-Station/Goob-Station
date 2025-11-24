using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Server.Singularity.Events;
using Content.Server.SurveillanceCamera;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Components;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Spawners;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Server-side system for slasher incorporeal mechanics.
/// </summary>
public sealed class SlasherIncorporealCameraSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly SharedHandheldLightSystem _handheld = default!;
    [Dependency] private readonly UnpoweredFlashlightSystem _unpowered = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;

    private EntityQuery<PointLightComponent> _pointLightQuery;
    private EntityQuery<PoweredLightComponent> _poweredLightQuery;
    private EntityQuery<HandheldLightComponent> _handheldLightQuery;
    private EntityQuery<UnpoweredFlashlightComponent> _unpoweredFlashlightQuery;
    private EntityQuery<ExpendableLightComponent> _expendableLightQuery;
    private EntityQuery<TimedDespawnComponent> _timedDespawnQuery;

    public override void Initialize()
    {
        base.Initialize();

        _pointLightQuery = GetEntityQuery<PointLightComponent>();
        _poweredLightQuery = GetEntityQuery<PoweredLightComponent>();
        _handheldLightQuery = GetEntityQuery<HandheldLightComponent>();
        _unpoweredFlashlightQuery = GetEntityQuery<UnpoweredFlashlightComponent>();
        _expendableLightQuery = GetEntityQuery<ExpendableLightComponent>();
        _timedDespawnQuery = GetEntityQuery<TimedDespawnComponent>();

        SubscribeLocalEvent<SlasherIncorporealCameraCheckEvent>(OnCameraCheck);
        SubscribeLocalEvent<SlasherIncorporealComponent, EventHorizonAttemptConsumeEntityEvent>(OnSingularityAttemptConsume);
        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherIncorporealEnteredEvent>(OnIncorporealEntered);
    }

    private void OnCameraCheck(ref SlasherIncorporealCameraCheckEvent args)
    {
        if (args.Cancelled)
            return;

        var slasher = GetEntity(args.Slasher);
        var range = args.Range;

        foreach (var other in _lookup.GetEntitiesInRange(slasher, range))
        {
            if (other == slasher)
                continue;

            // Require an active surveillance camera.
            if (!TryComp<SurveillanceCameraComponent>(other, out var cam) || !cam.Active)
                continue;

            if (_interaction.InRangeUnobstructed(other, slasher, range, CollisionGroup.Opaque))
            {
                args.Cancelled = true;
                return;
            }
        }
    }

    private void OnSingularityAttemptConsume(EntityUid uid, SlasherIncorporealComponent comp, ref EventHorizonAttemptConsumeEntityEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancelled = true;
    }

    private void OnIncorporealEntered(Entity<SlasherIncorporealComponent> ent, ref SlasherIncorporealEnteredEvent args)
    {
        // Extinguish any fires on the slasher
        if (TryComp<FlammableComponent>(ent, out var flammable))
        {
            _flammable.Extinguish(ent, flammable);
        }

        DisableLightsInArea(ent);
    }

    private void DisableLightsInArea(Entity<SlasherIncorporealComponent> ent)
    {
        var mapCoords = _transform.GetMapCoordinates(ent);

        // Get all entities in range and disable their lights
        foreach (var light in _lookup.GetEntitiesInRange(mapCoords, ent.Comp.LightDisableRange))
        {
            TryDisableLight(light);
        }
    }

    private void TryDisableLight(EntityUid uid)
    {
        // Must have a point light to be considered
        if (!_pointLightQuery.HasComp(uid))
            return;

        // Break lightbulbs
        if (_poweredLightQuery.TryComp(uid, out var light))
        {
            _light.TryDestroyBulb(uid, light);
            return;
        }

        // Handle handheld lights (flashlights, etc.)
        if (_handheldLightQuery.TryComp(uid, out var handheldLight))
        {
            _handheld.SetActivated(uid, false, handheldLight);
            return;
        }

        // Handle unpowered flashlights (PDAs, etc.)
        if (_unpoweredFlashlightQuery.TryComp(uid, out var unpoweredFlashlight))
        {
            if (unpoweredFlashlight.LightOn)
                _unpowered.TryToggleLight(uid, unpoweredFlashlight.ToggleActionEntity);
            return;
        }

        // Handle expendable lights (flares, glowsticks, etc.)
        if (_expendableLightQuery.TryComp(uid, out var expendableLight))
        {
            expendableLight.CurrentState = ExpendableLightState.Fading;
            expendableLight.StateExpiryTime = 0;
            return;
        }

        // Handle timed despawn items (flare gun projectiles)
        if (_timedDespawnQuery.TryComp(uid, out var timedDespawn))
        {
            timedDespawn.Lifetime = 0;
            return;
        }
    }
}
