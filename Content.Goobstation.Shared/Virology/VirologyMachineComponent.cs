using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;


namespace Content.Goobstation.Shared.Virology;

[RegisterComponent]
public sealed partial class VirologyMachineComponent : Component
{
    [DataField]
    public const string SwabSlotId = "disease_swab_slot";

    [DataField]
    public ItemSlot SwabSlot = new();

    [DataField]
    public EntProtoId PaperPrototype = "DiagnosisReportPaper";

    [DataField]
    public SoundSpecifier AnalyzedSound = new SoundPathSpecifier("/Audio/Machines/diagnoser_printing.ogg");

    [DataField, ViewVariables]
    public TimeSpan AnalysisDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public string? IdleState;

    [DataField]
    public string? RunningState;
}
