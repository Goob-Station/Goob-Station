using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Particles;

/// <summary>
/// Emits a particle effect while this entity is in flight after being thrown.
/// The emitter stops when the entity lands or is deleted.
/// </summary>
[RegisterComponent]
public sealed partial class ParticleOnThrownComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ParticleEffectPrototype> Effect;

    [DataField]
    public Color? ColorOverride;
}
