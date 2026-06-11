// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Particles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Particles;

public sealed class SpawnParticlesEffectSystem : SharedSpawnParticlesEffectSystem
{
    [Dependency] private readonly ParticleSystem _particles = default!;

    protected override void SpawnParticles(ProtoId<ParticleEffectPrototype> particleProto, EntityUid target, Color? color, bool attached)
    {
        base.SpawnParticles(particleProto, target, color, attached);

        _particles.CreateParticle(particleProto, target, color, attached);
    }
}
