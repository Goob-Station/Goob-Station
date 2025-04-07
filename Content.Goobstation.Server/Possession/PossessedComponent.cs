using Robust.Shared.Network;

namespace Content.Goobstation.Server.Possession;

[RegisterComponent]
public sealed partial class PossessedComponent : Component
{
    [DataField]
    public NetUserId OriginalMindId { get; set; }
    [DataField]
    public NetUserId PossessorMindId { get; set; }
    [DataField]
    public EntityUid PossessorOriginalEntity { get; set; }

    [DataField]
    public TimeSpan PossessionEndTime { get; set; }

    [DataField]
    public bool WasPacified;
}
