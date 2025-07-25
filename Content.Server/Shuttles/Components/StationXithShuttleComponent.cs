using Robust.Shared.Utility;



using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// GridSpawnComponent but for the star shuttle
/// <remarks>
/// This exists so we don't need to make 1 change to GridSpawn for every single station's unique shuttles.
/// </remarks>
/// </summary>
[RegisterComponent]
public sealed partial class StationXithShuttleComponent : Component
{
    [DataField(required: true)]
    public ResPath Path = new("/Maps/Shuttles/shepship.yml");
}
