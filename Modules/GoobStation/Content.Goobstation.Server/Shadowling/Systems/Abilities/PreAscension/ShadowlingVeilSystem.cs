using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Spawners;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles Veil, a re-skinned emp
/// </summary>
public sealed class ShadowlingVeilSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly SharedHandheldLightSystem _handheld = default!;
    [Dependency] private readonly UnpoweredFlashlightSystem _unpowered = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly TagSystem _tag = default!;

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

        SubscribeLocalEvent<ShadowlingVeilComponent, VeilEvent>(OnVeilActivate);
        SubscribeLocalEvent<ShadowlingVeilComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingVeilComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingVeilComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingVeilComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnVeilActivate(EntityUid uid, ShadowlingVeilComponent component, VeilEvent args)
    {
        if (args.Handled)
            return;

        // its just emp but better
        foreach (var light in _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(args.Performer), component.Range))
        {
            TryDisableLights(light, component);
        }

        args.Handled = true;
    }

    private void TryDisableLights(EntityUid uid, ShadowlingVeilComponent component)
    {
        if (!_pointLightQuery.HasComp(uid))
            return;

        if (_poweredLightQuery.TryComp(uid, out var light))
            _light.TryDestroyBulb(uid, light); // listen, this will make janitor a good role during slings

        if (_handheldLightQuery.TryComp(uid, out var handheldLight))
            _handheld.SetActivated(uid, false, handheldLight);

        // mostly for pdas
        if (_unpoweredFlashlightQuery.TryComp(uid, out var unpoweredFlashlight))
        {
            if (!unpoweredFlashlight.LightOn)
                return;

            _unpowered.TryToggleLight(uid, unpoweredFlashlight.ToggleActionEntity);
        }

        if (_expendableLightQuery.TryComp(uid, out var expendableLight)
            && !_tag.HasTag(uid, component.TorchTag))
        {
            expendableLight.CurrentState = ExpendableLightState.Fading;
            expendableLight.StateExpiryTime = 0;
            return;
        }

        // flare guns
        if (_timedDespawnQuery.TryComp(uid, out var timedDespawn))
            timedDespawn.Lifetime = 0;
    }
}
