using Content.Shared.Explosion;
using Content.Shared.Explosion.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Silicon.SecurityCyborg;

[RegisterComponent]
public sealed partial class SecurityCyborgComponent : Component
{
    [DataField]
    public ProtoId<ExplosionPrototype> ExplosionType = SharedExplosionSystem.DefaultExplosionPrototypeId;

    [DataField]
    public float TotalIntensity = 30f;

    [DataField]
    public float Slope = 20f;

    [DataField]
    public float MaxTileIntensity = 20f;
}