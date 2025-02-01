using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Disease;

/// <summary>
/// Base class for all disease effects
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DiseaseEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Severity = 1f;

    [DataField]
    public float Complexity = 4f;
}

// Deal damage to host
[RegisterComponent]
public sealed partial class DiseaseDamageEffectComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = default!;
}

// Decrease immunity progress on disease, use for incurable-once-developed diseases
[RegisterComponent]
public sealed partial class DiseaseFightImmunityEffectComponent : Component
{
    [DataField]
    public float Amount = -0.04f;
}
