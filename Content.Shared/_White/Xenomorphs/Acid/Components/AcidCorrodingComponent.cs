using Content.Shared.Damage;

namespace Content.Shared._White.Xenomorphs.Acid.Components;

[RegisterComponent]
public sealed partial class AcidCorrodingComponent : Component
{
    [DataField]
    public DamageSpecifier DamagePerSecond;

    [ViewVariables]
    public TimeSpan AcidExpiresAt;

    [ViewVariables]
    public TimeSpan NextDamageAt;

    [ViewVariables]
    public EntityUid Acid;
}
