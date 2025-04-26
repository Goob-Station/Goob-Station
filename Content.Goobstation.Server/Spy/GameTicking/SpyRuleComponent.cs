using Content.Shared.Dataset;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Spy.GameTicking;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class SpyRuleComponent : Component
{
    public readonly List<EntityUid> SpyMinds = new();

    [DataField]
    public ProtoId<AntagPrototype> TraitorPrototypeId = "Spy";

    [DataField]
    public ProtoId<NpcFactionPrototype> NanoTrasenFaction = "NanoTrasen";

    [DataField]
    public ProtoId<NpcFactionPrototype> SyndicateFaction = "Syndicate";

    [DataField]
    public ProtoId<DatasetPrototype> ObjectiveIssuers = "TraitorFlavor";

    [DataField]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/spy_start.ogg");
}
