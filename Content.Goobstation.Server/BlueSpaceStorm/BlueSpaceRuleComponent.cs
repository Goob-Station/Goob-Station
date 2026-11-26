using Content.Server.GameTicking.Rules;
using Content.Goobstation.Common.BluespaceStorm;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.BluespaceStorm;

[RegisterComponent]
public sealed partial class BluespaceRuleComponent : Component
{
    [DataField]
    public SoundSpecifier? DetectedAudio = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/blob_detected.ogg");

    [DataField]
    public List<EntProtoId> AvailablePortals = ["LavalandBluespacePortal", "PlantBluespacePortal", "FleshBluespacePortal", "SlimeBluespacePortal"];

    [DataField]
    public int PortalsRemaining = 0;
}
