using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Patrons;

[Prototype]
public sealed partial class PatronTierPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public int DatabaseId;

    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public ulong DiscordRole;

    [DataField]
    public int Priority;

    [DataField]
    public ProtoId<OocIconPrototype>? Icon;

    [DataField]
    public bool ShowOnCredits;

    [DataField]
    public bool GhostColor;

    [DataField]
    public bool GhostCosmetics;

    [DataField]
    public bool GhostParticles;

    [DataField]
    public bool LobbyMessage;

    [DataField]
    public bool RoundEndShoutout;

    [DataField]
    public List<ProtoId<PatronTierPrototype>> OocIconTiers = new();
}
