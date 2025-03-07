using Content.Shared._Goobstation.MartialArts.Components;
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
    public int MinDamageModifier;

    [DataField]
    public int MaxDamageModifier = 5;

    [DataField]
    public FixedPoint2 BaseDamageModifier;

    [DataField]
    public FixedPoint2 HarmAsStamina;

    [DataField]
    public bool RandomDamageModifier;

    [DataField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CQCMoves";

    [DataField]
    public List<LocId> RandomSayings = [];

    [DataField]
    public List<LocId> RandomSayingsDowned = [];
}
