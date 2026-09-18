using Content.Server.GameTicking.Rules;
using Content.Goobstation.Common.BlueSpaceStorm;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

[RegisterComponent]
public sealed partial class BlueSpaceRuleComponent : Component
{
    [DataField]
    public SoundSpecifier? DetectedAudio = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/blob_detected.ogg");

    [DataField]
    public List<EntProtoId> AvailablePortals = ["LavalandBluespacePortal", "PlantBluespacePortal", "FleshBluespacePortal", "SlimeBluespacePortal"];

    [DataField]
    public int PortalsRemaining = 4;
}