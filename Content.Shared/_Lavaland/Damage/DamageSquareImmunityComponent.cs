namespace Content.Shared._Lavaland.Damage;

/// <summary>
/// Actor having this component will not get damaged by damage squares.
/// </summary>
[RegisterComponent]
public sealed partial class DamageSquareImmunityComponent : Component
{
    [DataField]
    public TimeSpan HasImmunityUntil = TimeSpan.Zero;

    /// <summary>
    /// Setting this to true will ignore the timer and will make damage tile completely ignore an entity.
    /// </summary>
    [DataField]
    public bool IsImmune;
}
