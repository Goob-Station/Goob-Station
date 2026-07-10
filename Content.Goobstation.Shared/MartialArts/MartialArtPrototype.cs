// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts;

[Prototype("martialArt")]
public sealed partial class MartialArtPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public MartialArtsForms MartialArtsForm = MartialArtsForms.CloseQuartersCombat;

    [DataField]
    public int MinRandomDamageModifier;

    [DataField]
    public int MaxRandomDamageModifier = 5;

    [DataField]
    public FixedPoint2 BaseDamageModifier;

    [DataField]
    public ProtoId<DamageTypePrototype> DamageModifierType = "Blunt";

    [DataField]
    public bool RandomDamageModifier;

    [DataField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CQCMoves";

    [DataField]
    public List<LocId> RandomSayings = [];

    [DataField]
    public List<LocId> RandomSayingsDowned = [];

    [DataField]
    public GrabStage StartingStage = GrabStage.Soft;
}
