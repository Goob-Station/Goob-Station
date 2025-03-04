using Content.Shared._Goobstation.MartialArts.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts;

[Prototype("combo")]
public sealed partial class ComboPrototype : IPrototype
{
    [IdDataField] public string ID { get; private init; } = default!;

    [DataField("attacks", required: true)]
    public List<ComboAttackType> AttackTypes = new();

    //[DataField("weapon")] // Will be done later
    //public string? WeaponProtoId;

    [DataField("event", required: true)]
    public object? ResultEvent;
}

[Prototype("comboList")]
public sealed partial class ComboListPrototype : IPrototype
{
    [IdDataField] public string ID { get; private init; } = default!;

    [DataField("combos", required: true)]
    public List<ProtoId<ComboPrototype>> Combos = new();
}
