using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.Runes;

[Prototype("runeSelector")]
public sealed class RuneSelectorPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public EntProtoId Prototype { get; }

    [DataField]
    public float DrawTime { get; } = 4f;

    /// <summary>
    ///     Damage dealt on the rune drawing.
    /// </summary>
    [DataField]
    public DamageSpecifier DrawDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            ["Slash"] = 15,
        }
    };
}
