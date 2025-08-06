using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Xenomorphs.FaceHugger;

[RegisterComponent]
public sealed partial class FaceHuggerComponent : Component
{
    [DataField(required: true)]
    public string Slot;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public string InfectionSlotId = "xenomorph_larva";

    [DataField]
    public EntProtoId InfectionPrototype = "XenomorphInfection";

    [DataField]
    public int LarvaEmbryoCount = 1;

    [DataField]
    public List<MobState> AllowedPassiveDamageStates = new();

    [DataField]
    public DamageSpecifier PassiveDamage = new();
}
