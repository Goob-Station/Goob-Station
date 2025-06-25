using Content.Server.Actions;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared._EE.Shadowling;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Robust.Server.GameObjects;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Veil, a re-skinned emp
/// </summary>
public sealed class ShadowlingVeilSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly SharedHandheldLightSystem _handheld = default!;
    [Dependency] private readonly UnpoweredFlashlightSystem _unpowered = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingVeilComponent, VeilEvent>(OnVeilActivate);
    }

    private void OnVeilActivate(EntityUid uid, ShadowlingVeilComponent component, VeilEvent args)
    {
        // todo: handle visuals here

        // its just emp but better
        foreach (var light in _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(args.Performer), component.Range))
        {
            TryDisableLights(light);
        }

        _actions.StartUseDelay(args.Action);
    }

    private void TryDisableLights(EntityUid uid)
    {
        if (!HasComp<PointLightComponent>(uid))
            return;

        if (TryComp<PoweredLightComponent>(uid, out var light))
            _light.TryDestroyBulb(uid, light); // listen, this will make janitor a good role during slings

        if (TryComp<HandheldLightComponent>(uid, out var handheldLight))
        {
            _handheld.SetActivated(uid, false, handheldLight, true);
        }

        // mostly for pdas
        if (TryComp<UnpoweredFlashlightComponent>(uid, out var unpoweredFlashlight))
        {
            if (!unpoweredFlashlight.LightOn)
                return;
            _unpowered.TryToggleLight(uid, unpoweredFlashlight.ToggleActionEntity);
        }
    }
}
