namespace Content.Shared.Body.Components;

public sealed partial class BloodstreamComponent
{
    /// <summary>
    /// Goobstation - Prevents this entity from absorbing reagents from smoke/foam.
    /// </summary>
    [DataField]
    public bool SmokeImmune;

    /// <summary>
    /// Separated bleeding to base bleeding for simple mobs and abilities and bleeds
    /// based on BleedInflictors from wounds
    /// WoundMed Change
    [DataField, AutoNetworkedField]
    public float BleedAmountFromWounds;

    [DataField, AutoNetworkedField]
    public float BleedAmountNotFromWounds;
}
