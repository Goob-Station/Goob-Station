using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Virology;

[RegisterComponent]
public sealed partial class DiseaseAnalyzerComponent : Component
{
    [DataField]
    public const string SwabSlotId = "disease_swab_slot";

    [DataField]
    public ItemSlot SwabSlot = new();

    [DataField]
    public EntProtoId PaperPrototype = "DiagnosisReportPaper";

    [DataField]
    public SoundSpecifier AnalyzedSound = new SoundPathSpecifier("/Audio/Machines/ding.ogg");
}
