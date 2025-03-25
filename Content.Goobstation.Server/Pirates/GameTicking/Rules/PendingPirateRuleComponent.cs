using Content.Shared.Cargo;
using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Pirates.GameTicking.Rules;

[RegisterComponent]
public sealed partial class PendingPirateRuleComponent : Component
{
    [DataField] public float PirateSpawnTime = 300f; // 5 minutes
    public float PirateSpawnTimer = 0f;

    [DataField(required: true)] public EntProtoId RansomPrototype;

    // we need this for random announcements otherwise it'd be bland
    [DataField] public string LocAnnouncer = "irs";

    [DataField] public ProtoId<DatasetPrototype>? LocAnnouncers = null;

    [DataField] public float Ransom = 25000f;

    public CargoOrderData? Order;
}
