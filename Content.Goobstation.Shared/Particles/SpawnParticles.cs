// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Particles;

/// <summary>
/// Spawns particles at the current position of the entity.
/// </summary>
public sealed partial class SpawnParticles : EventEntityEffect<SpawnParticles>
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

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => null;
}

public abstract class SharedSpawnParticlesEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<SpawnParticles>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<SpawnParticles> args)
    {
        var effect = args.Effect.ParticleProto;
        var scale = args.Args is EntityEffectReagentArgs reagentArgs ? reagentArgs.Scale.Float() : 1f;
        var quantity = args.Effect.Number * (int)Math.Floor(scale);
        var color = args.Effect.Color;
        var attach = args.Effect.Attached;

        for (var i = 0; i < quantity; i++)
        {
            SpawnParticles(effect, args.Args.TargetEntity, color, attach);
        }
    }

    /// <summary>
    /// Virtual function to spawn particles via the client
    /// </summary>
    protected virtual void SpawnParticles(ProtoId<ParticleEffectPrototype> particleProto, EntityUid target, Color? color, bool attached) { }
}
