using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes.Apocalypse;

[RegisterComponent]
public sealed partial class CultRuneApocalypseComponent : Component
{
    [DataField]
    public float InvokeTime = 20;

    /// <summary>
    ///     If cult has less than this percent of current server population,
    ///     one of the possible events will be triggered.
    /// </summary>
    [DataField]
    public float CultistsThreshold = 0.15f;

    [DataField]
    public float EmpRange = 30f;

    [DataField]
    public float EmpEnergyConsumption = 10000;

    [DataField]
    public float EmpDuration = 180;

    /// <summary>
    ///     Was the rune already used or not.
    /// </summary>
    [DataField]
    public bool Used;

    [DataField]
    public Color UsedColor = Color.DimGray;

    /// <summary>
    ///     These events will be triggered on each rune activation.
    /// </summary>
    [DataField]
    public List<EntProtoId> GuaranteedEvents = new()
    {
        "PowerGridCheck",
        "SolarFlare"
    };

    /// <summary>
    ///     One of these events will be selected on each rune activation.
    ///     Stores the event and how many times it should be repeated.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, int> PossibleEvents = new()
    {
        ["ImmovableRodSpawn"] = 3,
        ["MimicVendorRule"] = 2,
        ["RatKingSpawn"] = 2,
        ["MeteorSwarm"] = 2,
        ["SpiderSpawn"] = 3, // more spiders
        ["AnomalySpawn"] = 4,
        ["KudzuGrowth"] = 2,
    };
}
