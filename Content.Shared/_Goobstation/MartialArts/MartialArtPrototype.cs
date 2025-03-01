using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts;

[Prototype("martialArt")]
public sealed partial class MartialArtPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = default!;

    [DataField]
    public MartialArtsForms MartialArtsForm = MartialArtsForms.CloseQuartersCombat;

    [DataField]
    public FixedPoint2 MinDamageModifier = 2.0;

    [DataField]
    public FixedPoint2 MaxDamageModifier = 3.0;

    [DataField]
    public bool RandomDamageModifier;

    [DataField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CQCMoves";
}
