// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Goobstation.Shared.Particles;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Particles.Visuals;

/// <summary>
/// Particles when entities are on fire.
/// </summary>
public sealed class FlammableParticleSystem : EntitySystem
{ /* flammable isnt shared yet gg.
    [Dependency] private readonly ParticleSystem _particles = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly ProtoId<ParticleEffectPrototype> FireEffect  = "SfFireContinuous";
    private static readonly ProtoId<ParticleEffectPrototype> SmokeEffect = "SfFireSmoke";

    private const float MaxStacks = 10f;

    private sealed class FireState
    {
        public ActiveEmitter? FireEmitter;
        public ActiveEmitter? SmokeEmitter;
        public bool OnFire;
    }

    private readonly Dictionary<EntityUid, FireState> _active = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlammableComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<FlammableComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnAppearanceChange(Entity<FlammableComponent> ent, ref AppearanceChangeEvent args)
    {
        if (!_appearance.TryGetData(ent, FireVisuals.OnFire, out bool onFire))
            onFire = false;

        if (!_appearance.TryGetData(ent, FireVisuals.FireStacks, out float stacks))
            stacks = 0f;

        if (!_active.TryGetValue(ent, out var state))
        {
            state = new FireState();
            _active[ent] = state;
        }

        if (onFire && !state.OnFire)
        {
            // Ignited: spawn emitter
            var coords = _transform.GetMapCoordinates(ent);
            state.SmokeEmitter = _particles.SpawnEffect(SmokeEffect, coords, ent.Owner);
            state.FireEmitter  = _particles.SpawnEffect(FireEffect,  coords, ent.Owner);

            if (state.SmokeEmitter != null) state.SmokeEmitter.Intensity = 1f;
            if (state.FireEmitter != null)  state.FireEmitter.Intensity  = 1f;

            state.OnFire = true;
        }
        else if (!onFire && state.OnFire)
        {
            // Extinguished: stop emitters
            StopState(state);
            state.OnFire = false;
        }

        // Update intensity on live emitters
        if (state.OnFire && state.FireEmitter != null)
        {
            var intensity = Math.Clamp(stacks / MaxStacks * 2f, 1f, 2f);
            if (state.FireEmitter != null)
                state.FireEmitter.Intensity = intensity;
            if (state.SmokeEmitter != null)
                state.SmokeEmitter.Intensity = intensity;
        }
    }

    private void OnShutdown(Entity<FlammableComponent> ent, ref ComponentShutdown args)
    {
        if (_active.Remove(ent, out var state))
            StopState(state);
    }

    private void StopState(FireState state)
    {
        if (state.FireEmitter != null)
        {
            state.FireEmitter.Exhausted = true;
            state.FireEmitter = null;
        }

        if (state.SmokeEmitter != null)
        {
            state.SmokeEmitter.Exhausted = true;
            state.SmokeEmitter = null;
        }
    }
    */
}
