// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Map;
using Content.Shared._EinsteinEngines.Flight.Events; // Goobstation

namespace Content.Shared.Gravity;

/// <summary>
/// Handles offsetting a sprite when there is no gravity
/// </summary>
public abstract class SharedFloatingVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SharedGravitySystem _gravity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FloatingVisualsComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<FloatingVisualsComponent, WeightlessnessChangedEvent>(OnWeightlessnessChanged);
        SubscribeLocalEvent<FloatingVisualsComponent, FlightEvent>(OnFlight); // Goobstation
    }

    /// <summary>
    /// Offsets a sprite with a linear interpolation animation
    /// </summary>
    public virtual void FloatAnimation(EntityUid uid, Vector2 offset, string animationKey, float animationTime, bool stop = false) { }

    protected bool CanFloat(Entity<FloatingVisualsComponent> entity)
    {
        entity.Comp.CanFloat = _gravity.IsWeightless(entity.Owner);
        Dirty(entity);
        return entity.Comp.CanFloat;
    }

    private void OnComponentStartup(Entity<FloatingVisualsComponent> entity, ref ComponentStartup args)
    {
        if (CanFloat(entity))
            FloatAnimation(entity, entity.Comp.Offset, entity.Comp.AnimationKey, entity.Comp.AnimationTime);
    }

    private void OnWeightlessnessChanged(Entity<FloatingVisualsComponent> entity, ref WeightlessnessChangedEvent args)
    {
        if (entity.Comp.CanFloat == args.Weightless)
            return;

        entity.Comp.CanFloat = CanFloat(entity);
        Dirty(entity);

        if (args.Weightless)
            FloatAnimation(entity, entity.Comp.Offset, entity.Comp.AnimationKey, entity.Comp.AnimationTime);
    }

    // Goobstation Start
    private void OnFlight(EntityUid uid, FloatingVisualsComponent component, FlightEvent args)
    {
        component.CanFloat = args.IsFlying;

        if (!args.IsFlying
            || !args.IsAnimated)
            return;

        FloatAnimation(uid, component.Offset, component.AnimationKey, component.AnimationTime);
    }
    // Goobstation End
}
