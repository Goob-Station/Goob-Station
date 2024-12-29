namespace Content.Server._Lavaland.Procedural.Components;

/// <summary>
/// Component that is used for the Lavaland outpost and
/// other habitable structures. Does not include small ruins.
/// </summary>
[RegisterComponent]
public sealed partial class LavalandMemberComponent : Component
{
    [DataField]
    public EntityUid LavalandMap;

    [DataField]
    public string SignalName;
}
