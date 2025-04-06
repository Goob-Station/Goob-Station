using Robust.Shared.Network;

namespace Content.Goobstation.Server.Posession;

[RegisterComponent]
public sealed partial class PossessedComponent : Component
{
    public NetUserId OriginalMindId { get; set; }
    public NetUserId PossessorMindId { get; set; }
    public EntityUid PossessorOriginalEntity { get; set; }

    public TimeSpan PosessionEndTime { get; set; }
}
