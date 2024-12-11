using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.Runes;

[Prototype("runeSelector")]
public sealed class RuneSelectorPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public EntProtoId Prototype;

    [DataField]
    public float DrawTime = 4f;

    [DataField]
    public bool RequireTargetDead;

    [DataField]
    public int RequiredTotalCultists = 1;

    /// <summary>
    ///     Damage dealt on the rune drawing.
    /// </summary>
    [DataField]
    public DamageSpecifier DrawDamage = new() { DamageDict = new() { ["Slash"] = 15 } };
}
