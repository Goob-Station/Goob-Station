// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 liltenhead <liltenhead@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.ParticleAccelerator.Components;
using Content.Server.Singularity.Components;
using Content.Shared.Projectiles;
using Content.Shared.Singularity.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server.ParticleAccelerator.EntitySystems;

public sealed partial class ParticleAcceleratorSystem
{
    private void FireEmitter(EntityUid uid, ParticleAcceleratorPowerState strength, ParticleAcceleratorEmitterComponent? emitter = null)
    {
        if (!Resolve(uid, ref emitter))
            return;

        var xformQuery = GetEntityQuery<TransformComponent>();
        if (!xformQuery.TryGetComponent(uid, out var xform))
        {
            Log.Error("ParticleAccelerator attempted to emit a particle without (having) a transform from which to base its initial position and orientation.");
            return;
        }

        var emitted = Spawn(emitter.EmittedPrototype, xform.Coordinates);

        if (xformQuery.TryGetComponent(emitted, out var particleXform))
            _transformSystem.SetLocalRotation(emitted, xform.LocalRotation, particleXform);

        if (TryComp<PhysicsComponent>(emitted, out var particlePhys))
        {
            var angle = _transformSystem.GetWorldRotation(uid, xformQuery);
            _physicsSystem.SetBodyStatus(emitted, particlePhys, BodyStatus.InAir);

            var velocity = angle.ToWorldVec() * 20f;
            if (TryComp<PhysicsComponent>(uid, out var phys))
                velocity += phys.LinearVelocity; // Inherit velocity from parent so if the clown has strapped a dozen engines to departures we don't outpace the particles.

            _physicsSystem.SetLinearVelocity(emitted, velocity, body: particlePhys);
        }

        if (TryComp<ProjectileComponent>(emitted, out var proj))
            _projectileSystem.SetShooter(emitted, proj, uid);

        if (TryComp<SinguloFoodComponent>(emitted, out var food))
        {
            // TODO: Unhardcode this.
            food.Energy = strength switch
            {
                ParticleAcceleratorPowerState.Standby => 0,
                ParticleAcceleratorPowerState.Level0 => 1,
                ParticleAcceleratorPowerState.Level1 => 2,
                ParticleAcceleratorPowerState.Level2 => 3,
                ParticleAcceleratorPowerState.Level3 => 6,
                _ => 0,
            } * 10;
        }

        if (TryComp<ParticleProjectileComponent>(emitted, out var particle))
            particle.State = strength;

        _appearanceSystem.SetData(emitted, ParticleAcceleratorVisuals.VisualState, strength);
    }
}