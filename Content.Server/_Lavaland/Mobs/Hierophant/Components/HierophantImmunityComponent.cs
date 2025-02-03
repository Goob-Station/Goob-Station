namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

/// <summary>
/// Actor having this component will not get damaged by hierophant squares.
/// </summary>
[RegisterComponent]
public sealed partial class HierophantImmunityComponent : Component
{
    [DataField]
    public TimeSpan HasImmunityUntil = TimeSpan.Zero;
}
