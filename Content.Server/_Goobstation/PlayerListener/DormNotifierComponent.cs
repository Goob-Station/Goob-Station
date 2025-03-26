using System.Linq;

namespace Content.Server._Goobstation.PlayerListener;

[RegisterComponent]
public sealed partial class DormNotifierComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<Condemnation> Potential = [];

    /// <summary>
    /// Stores sessions that have been found to be engaging in dorm activity
    /// </summary>
    public HashSet<Condemnation> Condemned = [];
}

public sealed class Condemnation(EntityUid marker, HashSet<EntityUid> condemned)
{
    public EntityUid Marker = marker;
    public HashSet<EntityUid> Condemned = condemned;
}
