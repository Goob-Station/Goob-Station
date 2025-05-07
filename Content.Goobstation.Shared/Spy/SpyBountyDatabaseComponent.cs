using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Spy;

[RegisterComponent]
public sealed partial class SpyBountyDatabaseComponent : Component
{
    [ViewVariables]
    public List<SpyBountyData> Bounties = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ResetTime = TimeSpan.FromMinutes(1);

    [ViewVariables(VVAccess.ReadOnly)] // i really dont trust our shitmins
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextReset = TimeSpan.Zero;
}
