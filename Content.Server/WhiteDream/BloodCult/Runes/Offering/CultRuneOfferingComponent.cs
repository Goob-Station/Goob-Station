using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Server.WhiteDream.BloodCult.Runes.Offering;

[RegisterComponent]
public sealed partial class CultRuneOfferingComponent : Component
{
    /// <summary>
    ///     The lookup range for offering targets
    /// </summary>
    [DataField]
    public float OfferingRange = 0.5f;

    /// <summary>
    ///     The amount of cultists require to convert a living target.
    /// </summary>
    [DataField]
    public int ConvertInvokersAmount = 2;

    /// <summary>
    ///     The amount of cultists required to sacrifice a living target.
    /// </summary>
    [DataField]
    public int AliveSacrificeInvokersAmount = 3;

    /// <summary>
    ///     The amount of charges revive rune system should recieve on sacrifice/convert.
    /// </summary>
    [DataField]
    public int ReviveChargesPerOffering = 1;

    [DataField]
    public DamageSpecifier ConvertHealing = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            ["Brute"] = -40,
            ["Burn"] = -40
        }
    };
}
