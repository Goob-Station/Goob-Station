using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Disease;

/// <summary>
/// Base class for all disease effects
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public abstract partial class DiseaseEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Severity = 1f;

    [DataField]
    public float Complexity = 4f;
}

[RegisterComponent]
public sealed partial class DiseaseDamageEffectComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = default!;
}
