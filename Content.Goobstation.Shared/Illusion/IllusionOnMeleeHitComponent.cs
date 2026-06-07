using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Illusion;

[RegisterComponent]
public sealed partial class IllusionOnMeleeHitComponent : Component
{
    [DataField]
    public float Chance = 0.5f;

    [DataField]
    public float Lifetime = 10f;

    [DataField]
    public float HealthMultiplier = 0.4f;

    [DataField]
    public float DegradationRateOnClone = 0.5f;

    [DataField]
    public List<ProtoId<NpcFactionPrototype>> FactionWhitelist = new();

    [DataField]
    public ComponentRegistry Components = new();
}
