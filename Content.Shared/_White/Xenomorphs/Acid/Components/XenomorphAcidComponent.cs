using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Acid.Components;

[RegisterComponent]
public sealed partial class XenomorphAcidComponent : Component
{
    [DataField]
    public EntProtoId AcidActionId = "ActionAcid";

    [DataField]
    public EntProtoId AcidId = "XenomorphAcid";

    [DataField]
    public TimeSpan AcidLifeTime = TimeSpan.FromSeconds(100);

    [DataField]
    public DamageSpecifier DamagePerSecond;

    [ViewVariables]
    public EntityUid? AcidAction;
}
