using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Disease;

/// <summary>
/// Base class for all disease effects
/// </summary>
public abstract partial class DiseaseEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Severity = 1f;
}

public sealed partial class DiseaseDamageEffectComponent : DiseaseEffectComponent
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier damage;
}
