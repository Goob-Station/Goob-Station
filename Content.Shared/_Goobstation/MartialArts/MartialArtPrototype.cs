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
    public FixedPoint2 DamageModifier = 2.0;

    [DataField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CQCMoves";
}
