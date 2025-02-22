namespace Content.Server._Lavaland.Tendril.Components;

/// <summary>
/// A mob created by a tendril. Upon death, it is removed from its spawn list
/// </summary>
[RegisterComponent]
public sealed partial class TendrilMobComponent : Component
{
    public EntityUid? Tendril;
}
