using System.Threading;
using Content.Shared.Antag;
using Content.Shared.FixedPoint;
using Content.Shared.Language;
using Content.Shared.Mind;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.BloodCultist;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultistComponent : Component, IAntagStatusIconComponent
{
    [DataField]
    public float HolyConvertTime = 15f;

    [DataField]
    public int MaximumAllowedEmpowers = 4;

    [DataField]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "BloodCultMember";

    [DataField]
    public bool IconVisibleToGhost { get; set; } = true;

    [DataField]
    public ProtoId<LanguagePrototype> CultLanguageId { get; set; } = "Eldritch";

    [ViewVariables, NonSerialized]
    public EntityUid? BloodSpear;

    [ViewVariables, NonSerialized]
    public Entity<MindComponent>? OriginalMind;

    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 RitesBloodAmount = FixedPoint2.Zero;

    public Color OriginalEyeColor = Color.White;

    public CancellationTokenSource? DeconvertToken { get; set; }
}
