using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class IgnoreCollisionComponent : Component
{
    [DataField("originalFixtures")]
    public Dictionary<string, (int Layer, int Mask)> OriginalFixtures = new();
}
