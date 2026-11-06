using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Xenomorphs.Acid.Components;

[RegisterComponent, NetworkedComponent]
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
