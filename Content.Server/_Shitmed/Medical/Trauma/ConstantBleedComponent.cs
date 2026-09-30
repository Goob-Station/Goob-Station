namespace Content.Server._Shitmed.Medical.Trauma;

[RegisterComponent]
public sealed partial class ConstantBleedComponent : Component
{
    /// <summary>
    /// Total constant bleed floor from all bloodloss traumas currently on this body.
    /// </summary>
    [DataField]
    public float Amount;
}
