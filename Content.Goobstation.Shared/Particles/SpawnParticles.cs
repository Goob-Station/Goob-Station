// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Particles;

/// <summary>
/// Spawns particles at the current position of the entity.
/// </summary>
public sealed partial class SpawnParticles : EntityEffectBase<SpawnParticles>
{
    /// <summary>
    /// The particles to spawn
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ParticleEffectPrototype> ParticleProto;

    /// <summary>
    /// If true, it will attach to the entity
    /// </summary>
    [DataField]
    public bool Attached;

    /// <summary>
    /// Amount of particles we're spawning
    /// </summary>
    [DataField]
    public int Number = 1;

    /// <summary>
    /// If set, it will override the colour of the particle
    /// </summary>
    [DataField]
    public Color? Color;
}

public class EntityEffectBase<T>
{
}

public abstract class SharedSpawnParticlesEffectSystem : EntityEffectSystem<TransformComponent, SpawnParticles>
{
    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<SpawnParticles> args)
    {
        var effect = args.Effect.ParticleProto;
        var quantity = args.Effect.Number * (int)Math.Floor(args.Scale);
        var color = args.Effect.Color;
        var attach = args.Effect.Attached;

        for (int i = 0; i < quantity; i++)
        {
            SpawnParticles(effect, ent.Owner, color, attach);
        }
    }

    /// <summary>
    /// Virtual function to spawn particles via the client
    /// </summary>
    protected virtual void SpawnParticles(ProtoId<ParticleEffectPrototype> particleProto, EntityUid target, Color? color, bool attached) { }
}
