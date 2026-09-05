using Content.Shared._White.Xenomorphs.Caste;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Queen;

[RegisterComponent]
public sealed partial class XenomorphQueenComponent : Component
{
    /// <summary>
    /// Mob that will be spawned when queen used evolve action
    /// </summary>
    [DataField]
    public EntProtoId MobRoyalCaste = "MobXenomorphPraetorian";

    /// <summary>
    /// The royal caste
    /// </summary>
    [DataField]
    public ProtoId<XenomorphCastePrototype> RoyalCaste = "Praetorian";

    /// <summary>
    /// Caste whitelist that can evolve to RoyalCaste
    /// </summary>
    [DataField]
    public List<ProtoId<XenomorphCastePrototype>> CasteWhitelist = new() { "Drone", "Hunter", "Sentinel" };

    /// <summary>
    /// Amount of time need to evo
    /// </summary>
    [DataField]
    public TimeSpan EvolutionDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Amount of plasma needed to evolve
    /// </summary>
    [DataField]
    public float EvolveCost = 500f;

    /// <summary>
    /// The message that will relayed to every xeno when queen died
    /// </summary>
    [DataField]
    public LocId QueenDeathMessage = "xenomorphs-queen-died";

    /// <summary>
    /// Sound played when queen died
    /// </summary>
    [DataField]
    public SoundPathSpecifier QueenDeathSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_died.ogg");

    /// <summary>
    /// The sender name
    /// </summary>
    [DataField]
    public string QueenDeathAnnouncement = "Xenomorph Hivemind";

    /// <summary>
    /// The announcement color
    /// </summary>
    [DataField]
    public Color AnnouncementColor = Color.Red;
}
